﻿using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

namespace Common.Harmony
{
	using CIEnumerable = IEnumerable<CodeInstruction>;
	using CIList = List<CodeInstruction>;
	using CIPredicate = Predicate<CodeInstruction>;

	static partial class HarmonyExtensions
	{
		public static bool isLDC<T>(this CodeInstruction ci, T val) => ci.isOp(CIHelper.LdcOpCode.get<T>(), val);

		public static bool isOp(this CodeInstruction ci, OpCode opcode, object operand = null) =>
			ci.opcode == opcode && (operand == null || Equals(ci.operand, operand));

		// for local variables ops
		public static bool isOpLoc(this CodeInstruction ci, OpCode opcode, int index) =>
			ci.opcode == opcode && ((ci.operand as LocalBuilder)?.LocalIndex == index);

		public static int findIndexForLast(this CIList list, params CIPredicate[] predicates)
		{
			int index = 0;

			foreach (var p in predicates)
				if ((index = list.FindIndex(index, p)) == -1)
					return -1;

			return index;
		}
	}

	static partial class CIHelper // CodeInstruction sequences manipulation methods
	{
		public static CodeInstruction emitCall<T>(T func) where T: Delegate
		{
			Debug.assert(func?.Method != null);
			Debug.assert(func.Method.IsStatic, $"CIHelper.emitCall: method {func.Method} is not static!");

			return new CodeInstruction(OpCodes.Call, func.Method);
		}

		#region CIList methods

		// makes new list with cloned CodeInstructions
		public static CIList copyCIList(CIList list) => list.Select(ci => ci.Clone()).ToList();

		// makes list with CodeInstructions from various objects (see 'switch' for object types)
		public static CIList toCIList(params object[] cins)
		{
			var list = new CIList();

			foreach (var i in cins)
			{
				switch (i)
				{
					case CIEnumerable ciList:
						list.AddRange(ciList);
						break;
					case CodeInstruction ci:
						list.Add(ci);
						break;
					case OpCode opcode:
						list.Add(new CodeInstruction(opcode));
						break;
					case object operand: // if none of the above, it's probably operand for the last instruction
						Debug.assert(list.Count > 0 && list[list.Count - 1].operand == null, $"toCIList: error while trying use {i} ({i.GetType()}) as operand");

						list[list.Count - 1].operand = operand;
						break;
					default:
						Debug.assert(false, $"toCIList: one of the params is null?");
						break;
				}
			}

			return list;
		}
		#endregion

		#region ciInsert
		// maxMatchCount = 0 for all predicate matches
		// indexOffset - change actual index from matched for insertion
		// if indexOffset is 0 than cinsToInsert will be inserted right before finded instruction
		// throws assert exception if there were no matches at all or if maxMatchCount > 0 and there were less predicate matches
		public static CIList ciInsert(CIEnumerable cins, CIPredicate predicate, int indexOffset, int maxMatchCount, params object[] cinsToInsert) =>
			ciInsert(cins.ToList(), predicate, indexOffset, maxMatchCount, cinsToInsert);

		// for just first predicate match (insert right after finded instruction)
		public static CIList ciInsert(CIEnumerable cins, CIPredicate predicate, params object[] cinsToInsert) =>
			ciInsert(cins.ToList(), predicate, cinsToInsert);

		// for just first predicate match (insert right after finded instruction)
		public static CIList ciInsert(CIList list, CIPredicate predicate, params object[] cinsToInsert) =>
			ciInsert(list, predicate, 1, 1, cinsToInsert);

		public static CIList ciInsert(CIList list, CIPredicate predicate, int indexOffset, int maxMatchCount, params object[] cinsToInsert)
		{
			bool anyInserts = false; // just for assert
			int index, index0 = 0;

			var listToInsert = toCIList(cinsToInsert);
			int indexIncrement = listToInsert.Count + Math.Max(1, indexOffset);

			while ((index = list.FindIndex(index0, predicate)) != -1 && (anyInserts = true))
			{
				ciInsert(list, index + indexOffset, listToInsert);
				Debug.assert(indexOffset <= 1 || list.FindIndex(index + 1, indexOffset - 1, predicate) == -1); // just in case if indexOffset > 1

				index0 = index + indexIncrement; // next after finded or next after inserted

				if (--maxMatchCount == 0)
					break;

				listToInsert = copyCIList(listToInsert); // better make copy if need to insert this more than once (there might be a problems with labels otherwise)
			}

			Debug.assert(anyInserts, $"ciInsert: no insertions were made");
			Debug.assert(maxMatchCount <= 0, $"ciInsert: matchCount {maxMatchCount}");

			return list;
		}

		public static CIList ciInsert(CIList list, int index, params object[] cinsToInsert) =>
			ciInsert(list, index, toCIList(cinsToInsert));

		public static CIList ciInsert(CIList list, int index, CIList listToInsert)
		{
			if (index >= 0 && index <= list.Count)
			{
				LabelClipboard.copyFrom(list, index); // copy labels from instruction where we insert

				list.InsertRange(index, listToInsert);

				LabelClipboard.pasteTo(list, index); // add copied labels to a new instruction at 'index'
			}
			else Debug.assert(false, $"ciInsert: CodeInstruction index is invalid ({index})");

			return list;
		}
		#endregion

		#region ciRemove
		// indexOffset - change actual index from matched for removing
		// countToRemove - instructions count to be removed
		public static CIList ciRemove(CIEnumerable cins, CIPredicate predicate, int indexOffset, int countToRemove) =>
			ciRemove(cins.ToList(), predicate, indexOffset, countToRemove);

		public static CIList ciRemove(CIList list, CIPredicate predicate, int indexOffset, int countToRemove)
		{
			int index = list.FindIndex(predicate);
			return ciRemove(list, (index == -1? -1: index + indexOffset), countToRemove);
		}

		public static CIList ciRemove(CIEnumerable cins, int index, int countToRemove) =>
			ciRemove(cins.ToList(), index, countToRemove);

		public static CIList ciRemove(CIList list, int index, int countToRemove)
		{
			if (index >= 0 && index + countToRemove <= list.Count)
			{
				LabelClipboard.copyFrom(list, index);

				list.RemoveRange(index, countToRemove);

				LabelClipboard.pasteTo(list, index);
			}
			else Debug.assert(false, "ciRemove: CodeInstruction index is invalid");

			return list;
		}
		#endregion

		#region ciReplace
		// replaces first matched CodeInstruction with cinsForReplace CodeInstructions
		public static CIList ciReplace(CIEnumerable cins, CIPredicate predicate, params object[] cinsForReplace) =>
			ciReplace(cins.ToList(), predicate, cinsForReplace);

		public static CIList ciReplace(CIList list, CIPredicate predicate, params object[] cinsForReplace) =>
			ciReplace(list, list.FindIndex(predicate), cinsForReplace);

		public static CIList ciReplace(CIList list, int index, params object[] cinsForReplace)
		{
			if (index >= 0)
			{
				CIList listToInsert = toCIList(cinsForReplace);
				ciInsert(list, index, listToInsert); // insert first, so we can copy labels in ciInsert
				ciRemove(list, index + listToInsert.Count, 1);
			}
			else Debug.assert(false, "ciReplace: CodeInstruction index is invalid");

			return list;
		}
		#endregion

		#region label clipboard
		static class LabelClipboard
		{
			static List<Label> labels;

			public static void copyFrom(CIList list, int index)
			{
				Debug.assert(labels == null);

				if (index != list.Count && list[index].labels.Count > 0)
				{
					labels = new List<Label>(list[index].labels);
					list[index].labels.Clear();
				}
			}

			public static void pasteTo(CIList list, int index)
			{
				if (labels != null)
					list[index].labels.AddRange(labels);

				labels = null;
			}
		}
		#endregion

		#region LdcOpCode
		// helper class for getting LDC opcode based on number type
		// https://stackoverflow.com/questions/600978/how-to-do-template-specialization-in-c-sharp
		public static class LdcOpCode
		{
			interface IGetOpCode<T> { OpCode get(); }

			class GetOpCode<T>: IGetOpCode<T>
			{
				class GetOpSpec: IGetOpCode<float>, IGetOpCode<double>, IGetOpCode<int>, IGetOpCode<sbyte>
				{
					public static readonly GetOpSpec S = new GetOpSpec();

					OpCode IGetOpCode<float>.get()  => OpCodes.Ldc_R4;
					OpCode IGetOpCode<double>.get() => OpCodes.Ldc_R8;
					OpCode IGetOpCode<int>.get()    => OpCodes.Ldc_I4;
					OpCode IGetOpCode<sbyte>.get()  => OpCodes.Ldc_I4_S;
				}

				public static readonly IGetOpCode<T> S = GetOpSpec.S as IGetOpCode<T> ?? new GetOpCode<T>();

				OpCode IGetOpCode<T>.get() => OpCodes.Nop;
			}

			public static OpCode get<T>() => GetOpCode<T>.S.get();
		}
		#endregion
	}
}
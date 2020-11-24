namespace KianCommons.Patches {
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;
    using System.Reflection;
    using System.Linq;

    public static class TranspilerUtils {
        static bool VERBOSE => HelpersExtensions.VERBOSE;
        static void Log(object message) {
            KianCommons.Log.Info("TRANSPILER " + message);
        }

        public const BindingFlags ALL = BindingFlags.Public
        | BindingFlags.NonPublic
        | BindingFlags.Instance
        | BindingFlags.Static
        | BindingFlags.GetField
        | BindingFlags.SetField
        | BindingFlags.GetProperty
        | BindingFlags.SetProperty;

        /// <typeparam name="TDelegate">delegate type</typeparam>
        /// <returns>Type[] represeting arguments of the delegate.</returns>
        internal static Type[] GetParameterTypes<TDelegate>() where TDelegate : Delegate {
            return typeof(TDelegate).GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();
        }

        /// <summary>
        /// Gets directly declared method.
        /// </summary>
        /// <typeparam name="TDelegate">delegate that has the same argument types as the intented overloaded method</typeparam>
        /// <param name="type">the class/type where the method is delcared</param>
        /// <param name="name">the name of the method</param>
        /// <returns>a method or null when type is null or when a method is not found</returns>
        internal static MethodInfo DeclaredMethod<TDelegate>(Type type, string name)
            where TDelegate : Delegate {
            var args = GetParameterTypes<TDelegate>();
            var ret = AccessTools.DeclaredMethod(type, name, args);
            if (ret == null)
                Log($"failed to retrieve method {type}.{name}({args})");
            return ret;
        }

        /// <summary>
        /// like AccessTools.DeclaredMethod but throws suitable exception if method not found.
        /// </summary>
        internal static MethodInfo GetMethod(Type type, string name) =>
            AccessTools.DeclaredMethod(type, name) ?? throw new Exception($"Method not found: {type.Name}.{name}");

        internal static MethodInfo GetCoroutineMoveNext(Type declaringType, string name) {
            Type t = declaringType.GetNestedTypes(ALL)
                .Single(_t => _t.Name.Contains($"<{name}>"));
            return GetMethod(t,"MoveNext");
        }


        public static List<CodeInstruction> ToCodeList(this IEnumerable<CodeInstruction> instructions) {
            var originalCodes = new List<CodeInstruction>(instructions);
            var codes = new List<CodeInstruction>(originalCodes); // is this redundant?
            return codes;
        }

        [Obsolete("use harmony extension IsLdArg(GetArgLoc(method, name)) instead")]
        public static CodeInstruction GetLDArg(MethodInfo method, string argName, bool throwOnError = true) {
            if (!throwOnError && !HasParameter(method, argName))
                return null;
            byte idx = (byte)GetArgLoc(method, argName);
            if (idx == 0) {
                return new CodeInstruction(OpCodes.Ldarg_0);
            } else if (idx == 1) {
                return new CodeInstruction(OpCodes.Ldarg_1);
            } else if (idx == 2) {
                return new CodeInstruction(OpCodes.Ldarg_2);
            } else if (idx == 3) {
                return new CodeInstruction(OpCodes.Ldarg_3);
            } else {
                return new CodeInstruction(OpCodes.Ldarg_S, idx);
            }
        }

        /// <returns>
        /// returns the argument location to be used in LdArg instruction.
        /// </returns>
        public static byte GetArgLoc(MethodInfo method, string argName) {
            byte idx = (byte)GetParameterLoc(method, argName);
            if (!method.IsStatic)
                idx++; // first argument is object instance.
            return idx;
        }

        /// <summary>
        /// Post condtion: for instnace method add one to get argument location
        /// </summary>
        public static byte GetParameterLoc(MethodInfo method, string name) {
            var parameters = method.GetParameters();
            for (byte i = 0; i < parameters.Length; ++i) {
                if (parameters[i].Name == name) {
                    return i;
                }
            }
            throw new Exception($"did not found parameter with name:<{name}>");
        }

        public static bool HasParameter(MethodInfo method, string name) {
            foreach(var param in method.GetParameters()) {
                if (param.Name == name)
                    return true;
            }
            return false;
        }

        [Obsolete("use harmony extension `Is()` instead")]
        public static bool IsSameInstruction(this CodeInstruction a, CodeInstruction b) {
            if (a.opcode == b.opcode) {
                if (a.operand == b.operand) {
                    return true;
                }

                // This special code is needed for some reason because the == operator doesn't work on System.Byte
                return (a.operand is byte aByte && b.operand is byte bByte && aByte == bByte);
            } else {
                return false;
            }
        }


        [Obsolete("use harmony extension instead")]
        public static bool IsLdLoc(CodeInstruction instruction) {
            return (instruction.opcode == OpCodes.Ldloc_0 || instruction.opcode == OpCodes.Ldloc_1 ||
                    instruction.opcode == OpCodes.Ldloc_2 || instruction.opcode == OpCodes.Ldloc_3
                    || instruction.opcode == OpCodes.Ldloc_S || instruction.opcode == OpCodes.Ldloc
                );
        }

        [Obsolete("use harmony extension instead")]
        public static bool IsStLoc(CodeInstruction instruction) {
            return (instruction.opcode == OpCodes.Stloc_0 || instruction.opcode == OpCodes.Stloc_1 ||
                    instruction.opcode == OpCodes.Stloc_2 || instruction.opcode == OpCodes.Stloc_3
                    || instruction.opcode == OpCodes.Stloc_S || instruction.opcode == OpCodes.Stloc
                );
        }

        /// <summary>
        /// Get the instruction to load the variable which is stored here.
        /// </summary>
        public static CodeInstruction BuildLdLocFromStLoc(this CodeInstruction instruction) {
            if (instruction.opcode == OpCodes.Stloc_0) {
                return new CodeInstruction(OpCodes.Ldloc_0);
            } else if (instruction.opcode == OpCodes.Stloc_1) {
                return new CodeInstruction(OpCodes.Ldloc_1);
            } else if (instruction.opcode == OpCodes.Stloc_2) {
                return new CodeInstruction(OpCodes.Ldloc_2);
            } else if (instruction.opcode == OpCodes.Stloc_3) {
                return new CodeInstruction(OpCodes.Ldloc_3);
            } else if (instruction.opcode == OpCodes.Stloc_S) {
                return new CodeInstruction(OpCodes.Ldloc_S, instruction.operand);
            } else if (instruction.opcode == OpCodes.Stloc) {
                return new CodeInstruction(OpCodes.Ldloc, instruction.operand);
            } else {
                throw new Exception("instruction is not stloc! : " + instruction);
            }
        }

        public static CodeInstruction BuildStLocFromLdLoc(this CodeInstruction instruction) {
            if (instruction.opcode == OpCodes.Ldloc_0) {
                return new CodeInstruction(OpCodes.Stloc_0);
            } else if (instruction.opcode == OpCodes.Ldloc_1) {
                return new CodeInstruction(OpCodes.Stloc_1);
            } else if (instruction.opcode == OpCodes.Ldloc_2) {
                return new CodeInstruction(OpCodes.Stloc_2);
            } else if (instruction.opcode == OpCodes.Ldloc_3) {
                return new CodeInstruction(OpCodes.Stloc_3);
            } else if (instruction.opcode == OpCodes.Ldloc_S) {
                return new CodeInstruction(OpCodes.Stloc_S, instruction.operand);
            } else if (instruction.opcode == OpCodes.Ldloc) {
                return new CodeInstruction(OpCodes.Stloc, instruction.operand);
            } else {
                throw new Exception("instruction is not ldloc! : " + instruction);
            }
        }

        internal static string IL2STR(this IEnumerable<CodeInstruction> instructions) {
            string ret = "";
            foreach (var code in instructions) {
                ret += code + "\n";
            }
            return ret;
        }

        public class InstructionNotFoundException : Exception {
            public InstructionNotFoundException() : base() { }
            public InstructionNotFoundException(string m) : base(m) { }
        }

        /// <param name="count">Number of occurances. Negative count searches backward</param>
        public static int Search(
            this List<CodeInstruction> codes,
            Func<CodeInstruction, bool> predicate,
            int startIndex = 0, int count = 1, bool throwOnError=true) {
            try {
                return codes.Search(
                    (int i) => predicate(codes[i]),
                    startIndex: startIndex,
                    count: count,
                    throwOnError: throwOnError);
            } catch (InstructionNotFoundException) {
                throw new InstructionNotFoundException(" Did not found instruction");
            }
        }

        /// <param name="count">negative count searches backward</param>
        public static int Search(
            this List<CodeInstruction> codes,
            Func<int, bool> predicate,
            int startIndex = 0, int count = 1, bool throwOnError = true) {
            if (count == 0)
                throw new ArgumentOutOfRangeException("count can't be zero");
            int dir = count > 0 ? 1 : -1;
            int counter = Math.Abs(count);
            int n = 0;
            int index = startIndex;

            for (; 0 <= index && index < codes.Count; index += dir) {
                if (predicate(index)) {
                    if (++n == counter)
                        break;
                }
            }
            if (n != counter) {
                if (throwOnError == true)
                    throw new InstructionNotFoundException("Did not found instruction[s].");
                else {
                    if (VERBOSE)
                        Log("Did not found instruction[s].\n" + Environment.StackTrace);
                    return -1;
                }
            }
            if (VERBOSE)
                Log("Found : \n" + new[] { codes[index], codes[index + 1] }.IL2STR());
            return index;
        }


        public static int SearchInstruction(List<CodeInstruction> codes, CodeInstruction instruction, int index, int dir = +1, int counter = 1) {
            try {
                return SearchGeneric(codes, idx => IsSameInstruction(codes[idx], instruction), index, dir, counter);
            }
            catch (InstructionNotFoundException) {
                throw new InstructionNotFoundException(" Did not found instruction: " + instruction);
            }
        }

        public static int SearchGeneric(List<CodeInstruction> codes, Func<int,bool> predicate, int index, int dir = +1, int counter = 1, bool throwOnError=true) {
            
            int count = 0;
            for (; 0 <= index && index < codes.Count; index += dir) {
                if (predicate(index)) {
                    if (++count == counter)
                        break;
                }
            }
            if (count != counter) {
                if (throwOnError == true)
                    throw new InstructionNotFoundException(" Did not found instruction[s].");
                else {
                    if (VERBOSE)
                        Log("Did not found instruction[s].\n" + Environment.StackTrace);
                    return -1;
                }
            }
            if(VERBOSE)
                Log("Found : \n" + new[] { codes[index], codes[index + 1] }.IL2STR());
            return index;
        }

        [Obsolete("unreliable")]
        public static Label GetContinueLabel(List<CodeInstruction> codes, int index, int counter = 1, int dir = -1) {
            // continue command is in form of branch into the end of for loop.
            index = SearchGeneric(codes, idx => codes[idx].Branches(out _), index, dir: dir, counter: counter);
            return (Label)codes[index].operand;
        }

        [Obsolete("use harmoyn extension Branches() instead")]
        public static bool IsBR32(OpCode opcode) {
            return opcode == OpCodes.Br || opcode == OpCodes.Brtrue || opcode == OpCodes.Brfalse || opcode == OpCodes.Beq;
        }

        public static void MoveLabels(CodeInstruction source, CodeInstruction target) {
            // move labels
            var labels = source.labels;
            target.labels.AddRange((IEnumerable<Label>)labels);
            labels.Clear();
        }

        /// <summary>
        /// replaces one instruction at the given index with multiple instrutions
        /// </summary>
        public static void ReplaceInstructions(List<CodeInstruction> codes, CodeInstruction[] insertion, int index) {
            foreach (var code in insertion)
                if (code == null)
                    throw new Exception("Bad Instructions:\n" + insertion.IL2STR());
            if(VERBOSE)
                Log($"replacing <{codes[index]}>\nInsert between: <{codes[index - 1]}>  and  <{codes[index + 1]}>");

            MoveLabels(codes[index], insertion[0]);
            codes.RemoveAt(index);
            codes.InsertRange(index, insertion);

            if (VERBOSE)
                Log("Replacing with\n" + insertion.IL2STR());
            if (VERBOSE)
                Log("PEEK (RESULTING CODE):\n" + codes.GetRange(index - 4, insertion.Length + 8).IL2STR());
        }

        public static void InsertInstructions(List<CodeInstruction> codes, CodeInstruction[] insertion, int index, bool moveLabels=true) {
            foreach (var code in insertion)
                if (code == null)
                    throw new Exception("Bad Instructions:\n" + insertion.IL2STR());
            if (VERBOSE)
                Log($"Insert point:\n between: <{codes[index - 1]}>  and  <{codes[index]}>");

            MoveLabels(codes[index], insertion[0]);
            codes.InsertRange(index, insertion);

            if (VERBOSE)
                Log("\n" + insertion.IL2STR());
            if (VERBOSE)
                Log("PEEK:\n" + codes.GetRange(index - 4, insertion.Length+12).IL2STR());
        }
    }
}

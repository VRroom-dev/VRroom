using UnityEngine;

// ReSharper disable InconsistentNaming
public class SimpleVM {
	private readonly object[] vars;
	private int[] program;
	private int pc;
	
	public SimpleVM(SimpleProgram p) {
		program = p.Program;
		vars = new object[p.Vars];
	}

	public void Execute(int start) {
		pc = start;
		while (pc < program.Length) {
			int op = program[pc++];
			switch (op) {
				case 0: { // Exit
					return;
				}
				case 1: { // Jump
					pc = program[pc++];
					break;
				}
				case 2: { // SetVar
					int dst = program[pc++];
					int val = program[pc++];
					vars[dst] = val;
					break;
				}
				case 3: { // Copy
					int dst = program[pc++];
					int src = program[pc++];
					vars[dst] = vars[src];
					break;
				}
				case 4: { // Add
					int dst = program[pc++];
					int src1 = program[pc++];
					int src2 = program[pc++];
					vars[dst] = (int)vars[src1] + (int)vars[src2];
					break;
				}
				case 5: { // Subtract
					int dst = program[pc++];
					int src1 = program[pc++];
					int src2 = program[pc++];
					vars[dst] = (int)vars[src1] - (int)vars[src2];
					break;
				}
				case 6: { // Multiply
					int dst = program[pc++];
					int src1 = program[pc++];
					int src2 = program[pc++];
					vars[dst] = (int)vars[src1] * (int)vars[src2];
					break;
				}
				case 7: { // Divide
					int dst = program[pc++];
					int src1 = program[pc++];
					int src2 = program[pc++];
					vars[dst] = (int)vars[src1] / (int)vars[src2];
					break;
				}
				case 8: { // Equal
					int dst = program[pc++];
					int src1 = program[pc++];
					int src2 = program[pc++];
					vars[dst] = (int)vars[src1] == (int)vars[src2] ? 1 : 0;
					break;
				}
				case 9: { // NotEqual
					int dst = program[pc++];
					int src1 = program[pc++];
					int src2 = program[pc++];
					vars[dst] = (int)vars[src1] != (int)vars[src2] ? 1 : 0;
					break;
				}
				case 10: { // GreaterThan
					int dst = program[pc++];
					int src1 = program[pc++];
					int src2 = program[pc++];
					vars[dst] = (int)vars[src1] > (int)vars[src2] ? 1 : 0;
					break;
				}
				case 11: { // LessThan
					int dst = program[pc++];
					int src1 = program[pc++];
					int src2 = program[pc++];
					vars[dst] = (int)vars[src1] < (int)vars[src2] ? 1 : 0;
					break;
				}
				case 12: { // JumpIfTrue
					int condition = program[pc++];
					int jumpAddress = program[pc++];
					if ((int)vars[condition] != 0)
						pc = jumpAddress;
					break;
				}
				case 13: { // JumpIfFalse
					int condition = program[pc++];
					int jumpAddress = program[pc++];
					if ((int)vars[condition] == 0)
						pc = jumpAddress;
					break;
				}
			}
		}
	}
}

public class SimpleProgram : ScriptableObject {
	public int[] Entries;
	public int[] Program;
	public int Vars;
}
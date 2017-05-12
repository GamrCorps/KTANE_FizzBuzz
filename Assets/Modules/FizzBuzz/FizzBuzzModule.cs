using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using FizzBuzz;

public class FizzBuzzModule : MonoBehaviour {

	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMAudio KMAudio;
	public KMSelectable[] SelectionButtons;
	public KMSelectable SubmitButton;
	public TextMesh[] Labels;
	int[] ButtonStates = new int[3];
	int[,] Nums = new int[3,7];
	int[] Colors = new int[] {0, 1, 2};
	int[] Solutions = new int[3];
	string[] StateNames = new string[] {"NUMBER", "FIZZ", "BUZZ", "FIZZBUZZ"};
	int moduleId;
	static int moduleIdCounter = 1;
	Color[] ColorMats = new Color[]{Color.red, Color.green, Color.cyan, Color.yellow, Color.white};

	// STATES
	// 0: Word
	// 1: Fizz
	// 2: Buzz
	// 3: FizzBuzz

	// COLORS
	// 0: Red
	// 1: Green
	// 2: Blue
	// 3: Yellow
	// 4: White

	public int[,] bases = new int[,] {
		{7, 3, 2, 4, 5}, // 3+ Battery Holders Present
		{3, 4, 9, 2, 8}, // Serial & Parallel Ports Present
		{4, 5, 8, 8, 2}, // 3 Letters & 3 Digits in Serial Number
		{2, 3, 7, 9, 1}, // DVI & Sterio RCA Ports Present
		{6, 6, 1, 2, 8}, // 2 Strikes
		{1, 2, 2, 5, 3}, // 5+ Batteries Present
		{3, 1, 8, 3, 4}  // If None of the Above
	};

	void Start() {
		moduleId = moduleIdCounter++;

		GetComponent<KMBombModule>().OnActivate += OnActivate;

		for (int i = 0; i < 3; i++) {
			var j = i;
			SelectionButtons[i].OnInteract += delegate { HandlePress(j); return false; };
			int[] num = GenNum();
			for (int k = 0; k < 7; k++) {
				Nums[i,k] = num[k];
			}
			Labels[i].text = "";

			int color = Random.Range(0,5);
			Colors[i] = color;
			Labels[i].color = ColorMats[color];
		}

		SubmitButton.OnInteract += delegate { Submit(); return false; };

		//TODO: COLORS
	}

	void OnActivate() {
		for (int i = 0; i < 3; i++) {
			Labels[i].text = MakeString(i);
		}
		FindSolution();
	}

	bool HandlePress(int buttonNum) {
		KMAudio.PlaySoundAtTransform("tick", this.transform);
		SelectionButtons[buttonNum].AddInteractionPunch();

		int state = (ButtonStates[buttonNum] + 1) % 4;
		ButtonStates[buttonNum] = state;

		switch (state) {
		case 0:
			Labels[buttonNum].text = MakeString(buttonNum);
			break;
		case 1:
			Labels[buttonNum].text = "Fizz";
			break;
		case 2:
			Labels[buttonNum].text = "Buzz";
			break;
		case 3:
			Labels[buttonNum].text = "FizzBuzz";
			break;
		}

		//Debug.LogFormat("[FizzBuzz #{0}] Button {1} pressed. New display state is {2}", moduleId, buttonNum + 1, Labels[buttonNum].text);

		return false;
	}

	int[] GenNum() {
		int[] result = new int[7];
		for (int i = 0; i < 7; i++) {
			result[i] = Random.Range(0, 10);
		}
		return result;
	}

	string MakeString(int button) {
		string result = "";
		for(int i = 0; i < 7; i++){
			result += "" + Nums[button, i];
		}
		return result;
	}

	void FindSolution(bool extraInfo = true) {
		bool reprintSolutions = false;
		for (int n = 0; n < 3; n++) {
			int addNum = 0;
			if (BombInfo.GetBatteryHolderCount() >= 3) {
				addNum += bases[0, Colors[n]];
				if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} Condition Met: {2}", moduleId, n + 1, "3+ Battery Holders");
			}
			if (BombInfo.IsPortPresent(KMBombInfoExtensions.KnownPortType.Serial) && BombInfo.IsPortPresent(KMBombInfoExtensions.KnownPortType.Parallel)) {
				addNum += bases[1, Colors[n]];
				if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} Condition Met: {2}", moduleId, n + 1, "Serial & Parallel Ports Present");
			}
			if (Enumerable.Count(BombInfo.GetSerialNumberLetters()) == Enumerable.Count(BombInfo.GetSerialNumberNumbers())) {
				addNum += bases[2, Colors[n]];
				if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} Condition Met: {2}", moduleId, n + 1, "3 Letters & 3 Digits in Serial Number");
			}
			if (BombInfo.IsPortPresent(KMBombInfoExtensions.KnownPortType.DVI) && BombInfo.IsPortPresent(KMBombInfoExtensions.KnownPortType.StereoRCA)) {
				addNum += bases[3, Colors[n]];
				if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} Condition Met: {2}", moduleId, n + 1, "DVI-D & SterioRCA Ports Present");
			}
			if (BombInfo.GetStrikes() == 2) {
				addNum += bases[4, Colors[n]];
				if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} Condition Met: {2}", moduleId, n + 1, "2 Strikes on Bomb");
			}
			if (BombInfo.GetBatteryCount() >= 5) {
				addNum += bases[5, Colors[n]];
				if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} Condition Met: {2}", moduleId, n + 1, "5+ Batteries");
			}
			if (addNum == 0) {
				addNum += bases[6, Colors[n]];
				if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} Condition Met: {2}", moduleId, n + 1, "No Conditions Valid");
			}
			if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} adding number is {2}", moduleId, n + 1, addNum);

			int[] result = new int[7];
			int num = 0;
			int beforeNum = 0;
			for (int i = 0; i < 7; i++) {
				result[i] = ((Nums[n, i] + addNum) % 10);

				beforeNum += Nums[n, i];
				beforeNum *= 10;

				num += result[i];
				num *= 10;
			}

			beforeNum /= 10;
			num /= 10;

			if (extraInfo) Debug.LogFormat("[FizzBuzz #{0}] Button {1} original number is {2} and the final number is {3}", moduleId, n + 1, beforeNum, num);

			int solution = 0;
			if (num % 3 == 0) solution += 1;
			if (num % 5 == 0) solution += 2;

			if (!extraInfo && Solutions[n] != solution) {
				reprintSolutions = true;
				Debug.LogFormat("[FizzBuzz #{0}] Button {1} solution changed! It is now {2}", moduleId, n + 1, StateNames[solution]);
			}

			Solutions[n] = solution;
		}
		if (extraInfo || reprintSolutions) Debug.LogFormat("[FizzBuzz #{0}] Solutions: [{1}, {2}, {3}]", moduleId, StateNames[Solutions[0]], StateNames[Solutions[1]], StateNames[Solutions[2]]);
	}

	void Submit() {
		KMAudio.PlaySoundAtTransform("tick", this.transform);
		GetComponent<KMSelectable>().AddInteractionPunch();
		FindSolution(false);
		bool valid = true;
		for (int i = 0; i < 3; i++){
			if (Solutions[i] != ButtonStates[i]) valid = false;
		}
		if (valid) {
			Debug.LogFormat("[FizzBuzz #{0}] Submit button pressed. Module solved.", moduleId);
			Debug.LogFormat("[FizzBuzz #{0}] Submitted data: [{1}, {2}, {3}]; Solutions: [{4}, {5}, {6}]", moduleId, Labels[0].text, Labels[1].text, Labels[2].text, StateNames[Solutions[0]], StateNames[Solutions[1]], StateNames[Solutions[2]]);
			BombModule.HandlePass();
		} else {
			Debug.LogFormat("[FizzBuzz #{0}] Submit button pressed. Incorrect Solution.", moduleId);
			Debug.LogFormat("[FizzBuzz #{0}] Submitted data: [{1}, {2}, {3}]; Solutions: [{4}, {5}, {6}]", moduleId, Labels[0].text, Labels[1].text, Labels[2].text, StateNames[Solutions[0]], StateNames[Solutions[1]], StateNames[Solutions[2]]);
			BombModule.HandleStrike();
		}
		FindSolution(false);
	}

	KMSelectable[] ProcessTwitchCommand (string command) {
		command = command.Trim().ToLowerInvariant();
		var pieces = command.Split(new[] {' ', ','}, System.StringSplitOptions.RemoveEmptyEntries);

		if (pieces.Length < 2 || pieces[0] != "press")
			return null;

		var list = new List<KMSelectable>();
		for (int i = 1; i < pieces.Length; i++) {
			switch(pieces[i]) {
				case "t":
				case "top":
				case "u":
				case "up":
				case "upper":
				case "1":
				case "first":
				case "1st":
				case "one":
					list.Add(SelectionButtons[0]);
					break;

				case "m":
				case "middle":
				case "c":
				case "center":
				case "centre":
				case "2":
				case "second":
				case "2nd":
				case "two":
					list.Add(SelectionButtons[1]);
					break;

				case "b":
				case "bottom":
				case "d":
				case "down":
				case "l":
				case "lower":
				case "3":
				case "third":
				case "3rd":
				case "three":
					list.Add(SelectionButtons[2]);
					break;

				case "s":
				case "sub":
				case "submit":
				case "done":
					list.Add(SubmitButton);
					break;

				default:
					return null;
			}
		}

		return list.ToArray();
	}
}

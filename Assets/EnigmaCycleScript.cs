using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
public class EnigmaCycleScript : MonoBehaviour {

	const string keyboardLayout = "QWERTYUIOPASDFGHJKLZXCVBNM",
		blacklistedKeys = "", baseAlphabetUsed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", disarmText = "WELLDONE";
	private readonly string[] positionIndexes = { "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th" };
	private static Dictionary<int, string> enigmaRotors = new Dictionary<int, string>
	{
		{ 0, "XEOFMNLBJWHPYSRKVZUQGCIATD" },
		{ 1, "QCUWRXVGEATHFDLZPNYJKOBSIM" },
		{ 2, "PWGXFENSQTKLHYABMODCVZUJRI" },
		{ 3, "ONQFSXTDAILKBCJYUEPGMRWHZV" },
		{ 4, "JTZMDNRKLVWXCEBQPAOIFYGSHU" },
		{ 5, "FZXDTGVYWJKNSCMALBEQUROIPH" },
		{ 6, "EBQIGLDHVPFUYRASKZOXJNCTWM" },
		{ 7, "YSLBCXZGQANJDKMVFIEORUTWPH" },
	},
		enigmaReflectors = new Dictionary<int, string>
	{
		{ 0, "VGTFWKZMBODLASPYXJNCERHQUI" },
		{ 1, "LQGOJKRFYNSTCXZUMPWVAHDBEI" },
		{ 2, "HNKBLJWVECRZIMXPTAGYQODUFS" },
	};
	private static Dictionary<int, int[]> idxesToRotateOther = new Dictionary<int, int[]>
	{
		{ 0, new[] { 0, 13 } },
		{ 1, new[] { 5, 18 } },
		{ 2, new[] { 1, 14 } },
		{ 3, new[] { 3, 16 } },
		{ 4, new[] { 10, 23 } },
		{ 5, new[] { 12, 25 } },
		{ 6, new[] { 8, 21 } },
		{ 7, new[] { 4, 17 } },
	};


	private Dictionary<string, string> messageResponsePairs = new Dictionary<string, string>
	{
		{ "ABNORMAL", "ZILLIONS" }, { "AUTHORED", "GROANING" },
		{ "BACKDOOR", "PROVOKED" }, { "BOULDERS", "WACKIEST" },
		{ "CHANGING", "VOLATILE" }, { "CUMBERED", "WORKFLOW" },
		{ "DEBUGGED", "YABBERED" }, { "DODGIEST", "HUDDLING" },
		{ "EDITABLE", "FAIRYISM" }, { "EXCESSES", "ORDERING" },
		{ "FAIRYISM", "EDITABLE" }, { "FRAGMENT", "XANTHENE" },
		{ "GIBBERED", "KINDLING" }, { "GROANING", "EXCESSES" },
		{ "HEADACHE", "MOBILITY" }, { "HUDDLING", "LIKENESS" },
		{ "ILLUSORY", "QUITTERS" }, { "IRONICAL", "JUDGMENT" },
		{ "JOKINGLY", "NEUTRALS" }, { "JUDGMENT", "QUOTABLE" },
		{ "KEYNOTES", "XENOLITH" }, { "KINDLING", "SUBLIMES" },
		{ "LIKENESS", "DEBUGGED" }, { "LOCKOUTS", "IRONICAL" },
		{ "MOBILITY", "PHANTASM" }, { "MUFFLING", "HEADACHE" },
		{ "NEUTRALS", "BACKDOOR" }, { "NOTIONAL", "TARTNESS" },
		{ "OFFTRACK", "VARIANCE" }, { "ORDERING", "MUFFLING" },
		{ "PHANTASM", "BOULDERS" }, { "PROVOKED", "FRAGMENT" },
		{ "QUITTERS", "NOTIONAL" }, { "QUOTABLE", "DODGIEST" },
		{ "RHETORIC", "KEYNOTES" }, { "ROULETTE", "AUTHORED" },
		{ "SHUTDOWN", "CHANGING" }, { "SUBLIMES", "OFFTRACK" },
		{ "TARTNESS", "UGLINESS" }, { "TYPHONIC", "ROULETTE" },
		{ "UNPURGED", "ZAPPIEST" }, { "UGLINESS", "CUMBERED" },
		{ "VARIANCE", "RHETORIC" }, { "VOLATILE", "ILLUSORY" },
		{ "WACKIEST", "ABNORMAL" }, { "WORKFLOW", "GIBBERED" },
		{ "XENOLITH", "LOCKOUTS" }, { "XANTHENE", "SHUTDOWN" },
		{ "YABBERED", "JOKINGLY" }, { "YOURSELF", "UNPURGED" },
		{ "ZAPPIEST", "TYPHONIC" }, { "ZILLIONS", "YOURSELF" }
	};

	public KMBombModule modSelf;
	public KMAudio mAudio;
	public KMSelectable[] letterSelectables;
	public TextMesh[] dialLetters;
	public TextMesh display;
	public Transform[] dials;

	private KeyValuePair<string, string> selectedMessageResponsePair;

	string expectedResponse, encryptedDisplay = "ABCDEFGH";
	static int modIDCnt = 1;
	int modID;
	private int pressedLetterIdx;
	private Vector3[] keySelectableInitialPos, initialDialRotations;
	bool isSolving, allowInteractions, autoSolveEnforced;
	int[] curDialRotations = new int[8], assignedDialRotations = new int[8];
	readonly Vector3[] dialRotationSets = { Vector3.up * 120, Vector3.up * 45, Vector3.up * 45, Vector3.up * 45, Vector3.up * 30, Vector3.up * 30, Vector3.up * 30, Vector3.up * 45, };
    readonly int[] maxDialRotations = { 3, 8, 8, 8, 12, 12, 12, 8 };
	// Use this for initialization
	void QuickLog(string value)
    {
		Debug.LogFormat("[Enigma Cycle #{0}] {1}", modID, value);
    }
	void QuickLogDebug(string value)
    {
		Debug.LogFormat("<Enigma Cycle #{0}> {1}", modID, value);
    }
	void Start () {
		modID = modIDCnt++;
		for (var x = 0; x < letterSelectables.Length; x++)
        {
			int y = x;
			letterSelectables[x].OnInteract += delegate {
				mAudio.PlaySoundAtTransform("EnigmaPress", letterSelectables[y].transform);
				pressedLetterIdx = y;
				letterSelectables[y].AddInteractionPunch(0.125f);
				UpdateKeys();
				return false;
			};
			letterSelectables[x].OnInteractEnded += delegate {
				mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, letterSelectables[y].transform);
				pressedLetterIdx = -1;
				UpdateKeys();
				ProcessLetter(y);
			};
		}
		keySelectableInitialPos = letterSelectables.Select(a => new Vector3(a.transform.localPosition.x, a.transform.localPosition.y, a.transform.localPosition.z)).ToArray();
		initialDialRotations = dials.Select(a => new Vector3(a.transform.localEulerAngles.x, a.transform.localEulerAngles.y, a.transform.localEulerAngles.z)).ToArray();
		ResetModule();
	}

	void ProcessLetter(int idx)
    {
		if (!allowInteractions) return;
		if (idx < keyboardLayout.Length && idx >= 0)
		{
			if (!blacklistedKeys.Contains(keyboardLayout[idx]))
				display.text += keyboardLayout[idx];
		}
		else
			display.text = "";
		if (display.text.Length >= expectedResponse.Length)
        {
			if (string.IsNullOrEmpty(expectedResponse) || display.text.EqualsIgnoreCase(expectedResponse) || autoSolveEnforced)
            {
				mAudio.PlaySoundAtTransform("InputCorrect", letterSelectables[idx].transform);
				allowInteractions = false;
				isSolving = true;
				StartCoroutine(SpinDialsToSolve());
            }
			else
            {
				QuickLog(string.Format("{0} was incorrectly submitted. Resetting...", display.text));
				modSelf.HandleStrike();
				StartCoroutine(ShakeDisplay());
				display.color = Color.red;
				ResetModule();

            }
        }
	}

	void ResetModule()
    {
		allowInteractions = false;
		expectedResponse = "";
		if (messageResponsePairs.Any())
			selectedMessageResponsePair = messageResponsePairs.PickRandom();
		else
			selectedMessageResponsePair = new KeyValuePair<string, string>("EXAMPLES", "RESPONSE");

		for (var x = 0; x < assignedDialRotations.Length; x++)
        {
			assignedDialRotations[x] = Random.Range(0, maxDialRotations[x]);
        }
		QuickLog(string.Format("Dial rotations from left to right in a zig-zag formation are {0}", assignedDialRotations.Join(", ")));
		var enigmaWheelOffsetIndexes = assignedDialRotations.TakeLast(4).Take(3).ToArray();
		var enigmaWheelRotorIdx = assignedDialRotations.Take(4).Skip(1).ToArray();
		var enigmaWheelReflectorIdx = assignedDialRotations.First();
		var remainingDialRotation = assignedDialRotations.Last();
		var debugLetterSequence = new List<List<char>>();
		encryptedDisplay = "";
		// Modify the offsets if a condition is met
		
		for (var x = 0; x < enigmaWheelOffsetIndexes.Length; x++)
        {
			if (x + 5 == remainingDialRotation)
				enigmaWheelOffsetIndexes[x] = 25 - enigmaWheelOffsetIndexes[x];
			else
				enigmaWheelOffsetIndexes[x]++;
        }
		// Enigma Cipher encrypting message key
		for (var y = 0; y < selectedMessageResponsePair.Key.Length; y++)
        {
			var resultingCharOrder = new List<char>();
			var curChar = selectedMessageResponsePair.Key[y];
			resultingCharOrder.Add(curChar);
			var curIdx = baseAlphabetUsed.IndexOf(curChar);
			// Follow down the rotors
			for (var x = enigmaWheelRotorIdx.Length - 1; x >= 0; x--)
            {
				var curRotorOffset = enigmaWheelOffsetIndexes[x];
				var curRotorUsed = enigmaRotors[enigmaWheelRotorIdx[x]];

				var shiftedRotorTop = curRotorUsed.Substring(curRotorOffset) + curRotorUsed.Substring(0, curRotorOffset);
				var shiftedRotorBottom = baseAlphabetUsed.Substring(curRotorOffset) + baseAlphabetUsed.Substring(0, curRotorOffset);
				curIdx = shiftedRotorBottom.IndexOf(shiftedRotorTop[curIdx]);
				resultingCharOrder.Add(shiftedRotorBottom[curIdx]);
			}
			// Reflect
			resultingCharOrder.Add(baseAlphabetUsed[curIdx]);
			var reflectorUsed = enigmaReflectors[enigmaWheelReflectorIdx];
			if (remainingDialRotation == 1)
            {
				curIdx = 25 - curIdx;
				var reflectionIdx = reflectorUsed.IndexOf(baseAlphabetUsed[curIdx]);
				if (reflectionIdx % 2 == 0)
                    curIdx = baseAlphabetUsed.IndexOf(reflectorUsed[reflectionIdx + 1]);
				else
					curIdx = baseAlphabetUsed.IndexOf(reflectorUsed[reflectionIdx - 1]);
				curIdx = 25 - curIdx;
			}
			else
            {
				var reflectionIdx = reflectorUsed.IndexOf(baseAlphabetUsed[curIdx]);
				if (reflectionIdx % 2 == 0)
					curIdx = baseAlphabetUsed.IndexOf(reflectorUsed[reflectionIdx + 1]);
				else
					curIdx = baseAlphabetUsed.IndexOf(reflectorUsed[reflectionIdx - 1]);
			}
			resultingCharOrder.Add(baseAlphabetUsed[curIdx]);
			// Follow up the rotors
			for (var x = 0; x < enigmaWheelRotorIdx.Length; x++)
			{
				var curRotorOffset = enigmaWheelOffsetIndexes[x];
				var curRotorUsed = enigmaRotors[enigmaWheelRotorIdx[x]];

				var shiftedRotorTop = curRotorUsed.Substring(curRotorOffset) + curRotorUsed.Substring(0, curRotorOffset);
				var shiftedRotorBottom = baseAlphabetUsed.Substring(curRotorOffset) + baseAlphabetUsed.Substring(0, curRotorOffset);
				curIdx = shiftedRotorTop.IndexOf(shiftedRotorBottom[curIdx]);
				resultingCharOrder.Add(shiftedRotorTop[curIdx]);
			}
			encryptedDisplay += baseAlphabetUsed[curIdx];
			resultingCharOrder.Add(baseAlphabetUsed[curIdx]);
			debugLetterSequence.Add(resultingCharOrder);
			// Shift the rotors for the next encryption
			if (idxesToRotateOther[enigmaWheelRotorIdx[1]].Contains(enigmaWheelOffsetIndexes[1]))
            {
				enigmaWheelOffsetIndexes[0] = (enigmaWheelOffsetIndexes[0] + (remainingDialRotation == 4 ? 25 : 1) ) % 26;
				enigmaWheelOffsetIndexes[1] = (enigmaWheelOffsetIndexes[1] + (remainingDialRotation == 3 ? 25 : 1)) % 26;
			}
			else if (idxesToRotateOther[enigmaWheelRotorIdx[2]].Contains(enigmaWheelOffsetIndexes[2]))
				enigmaWheelOffsetIndexes[1] = (enigmaWheelOffsetIndexes[1] + (remainingDialRotation == 3 ? 25 : 1)) % 26;
			enigmaWheelOffsetIndexes[2] = (enigmaWheelOffsetIndexes[2] + (remainingDialRotation == 2 ? 25 : 1)) % 26;
		}
		// End Enigma Cipher encrypting message key
		QuickLog(string.Format("The encrypted message displayed is {0}", encryptedDisplay));
		for (var y = 0; y < debugLetterSequence.Count; y++)
		{
			debugLetterSequence[y].Reverse();
			QuickLog(string.Format("For the {1} letter, the path the the expert should take to decrypt is {0}", debugLetterSequence[y].Join(" -> "), positionIndexes[y]));
		}
		QuickLog(string.Format("The message is {0}", selectedMessageResponsePair.Key));
		QuickLog(string.Format("The response is {0}", selectedMessageResponsePair.Value));
		for (var x = 0; x < enigmaWheelOffsetIndexes.Length; x++)
		{
			if (x + 5 == remainingDialRotation)
				enigmaWheelOffsetIndexes[x] = 25 - assignedDialRotations[x + 4];
			else
				enigmaWheelOffsetIndexes[x] = assignedDialRotations[x + 4] + 1;
		}
		// Start Enigma Cipher encrypting response
		debugLetterSequence.Clear();
		for (var y = 0; y < selectedMessageResponsePair.Value.Length; y++)
		{
			var resultingCharOrder = new List<char>();
			var curChar = selectedMessageResponsePair.Value[y];
			resultingCharOrder.Add(curChar);
			var curIdx = baseAlphabetUsed.IndexOf(curChar);
			// Follow down the rotors
			for (var x = enigmaWheelRotorIdx.Length - 1; x >= 0; x--)
			{
				var curRotorOffset = enigmaWheelOffsetIndexes[x];
				var curRotorUsed = enigmaRotors[enigmaWheelRotorIdx[x]];

				var shiftedRotorTop = curRotorUsed.Substring(curRotorOffset) + curRotorUsed.Substring(0, curRotorOffset);
				var shiftedRotorBottom = baseAlphabetUsed.Substring(curRotorOffset) + baseAlphabetUsed.Substring(0, curRotorOffset);
				curIdx = shiftedRotorBottom.IndexOf(shiftedRotorTop[curIdx]);
				resultingCharOrder.Add(shiftedRotorBottom[curIdx]);
			}
			// Reflect
			var reflectorUsed = enigmaReflectors[enigmaWheelReflectorIdx];
			if (remainingDialRotation == 1)
			{
				curIdx = 25 - curIdx;
				var reflectionIdx = reflectorUsed.IndexOf(baseAlphabetUsed[curIdx]);
				if (reflectionIdx % 2 == 0)
					curIdx = baseAlphabetUsed.IndexOf(reflectorUsed[reflectionIdx + 1]);
				else
					curIdx = baseAlphabetUsed.IndexOf(reflectorUsed[reflectionIdx - 1]);
				curIdx = 25 - curIdx;
			}
			else
			{
				var reflectionIdx = reflectorUsed.IndexOf(baseAlphabetUsed[curIdx]);
				if (reflectionIdx % 2 == 0)
					curIdx = baseAlphabetUsed.IndexOf(reflectorUsed[reflectionIdx + 1]);
				else
					curIdx = baseAlphabetUsed.IndexOf(reflectorUsed[reflectionIdx - 1]);
			}
			resultingCharOrder.Add(baseAlphabetUsed[curIdx]);
			// Follow up the rotors
			for (var x = 0; x < enigmaWheelRotorIdx.Length; x++)
			{
				var curRotorOffset = enigmaWheelOffsetIndexes[x];
				var curRotorUsed = enigmaRotors[enigmaWheelRotorIdx[x]];

				var shiftedRotorTop = curRotorUsed.Substring(curRotorOffset) + curRotorUsed.Substring(0, curRotorOffset);
				var shiftedRotorBottom = baseAlphabetUsed.Substring(curRotorOffset) + baseAlphabetUsed.Substring(0, curRotorOffset);
				curIdx = shiftedRotorTop.IndexOf(shiftedRotorBottom[curIdx]);
				resultingCharOrder.Add(shiftedRotorTop[curIdx]);
			}
			expectedResponse += baseAlphabetUsed[curIdx];
			resultingCharOrder.Add(baseAlphabetUsed[curIdx]);
			debugLetterSequence.Add(resultingCharOrder);
			// Shift the rotors for the next encryption
			if (idxesToRotateOther[enigmaWheelRotorIdx[1]].Contains(enigmaWheelOffsetIndexes[1]))
			{
				enigmaWheelOffsetIndexes[0] = (enigmaWheelOffsetIndexes[0] + (remainingDialRotation == 4 ? 25 : 1)) % 26;
				enigmaWheelOffsetIndexes[1] = (enigmaWheelOffsetIndexes[1] + (remainingDialRotation == 3 ? 25 : 1)) % 26;
			}
			else if (idxesToRotateOther[enigmaWheelRotorIdx[2]].Contains(enigmaWheelOffsetIndexes[2]))
				enigmaWheelOffsetIndexes[1] = (enigmaWheelOffsetIndexes[1] + (remainingDialRotation == 3 ? 25 : 1)) % 26;
			enigmaWheelOffsetIndexes[2] = (enigmaWheelOffsetIndexes[2] + (remainingDialRotation == 2 ? 25 : 1)) % 26;
		}
		for (var y = 0; y < debugLetterSequence.Count; y++)
		{
			QuickLog(string.Format("For the {1} letter, the path the module took to encrypt is {0}", debugLetterSequence[y].Join(" -> "), positionIndexes[y]));
		}
		// End Enigma Cipher encrypting response
		QuickLog(string.Format("The encrypted response to submit is {0}", expectedResponse));
		StartCoroutine(SpinDialsToInitial());
    }
	/*
	char EncryptWithEnigmaCipher(char input, string reflector, string[] wheels, int[] initWheelOffsets, bool logPath = false)
    {
		if (baseAlphabetUsed.Contains(input) && wheels.Length <= initWheelOffsets.Length)
        {
			var pathTaken = new List<char>();
			var curIdx = baseAlphabetUsed.IndexOf(input);
			pathTaken.Add(input);


			if (logPath)
				QuickLogDebug(pathTaken.Join(" -> "));
        }
		return input;
    }
	*/
	IEnumerator ShakeDisplay()
    {
		var lastPos = display.transform.localPosition;
		for (float x = 0; x < 1f; x += Time.deltaTime * 2)
		{
			yield return null;
			var curShake = Mathf.Sin(720 * x) * 0.075f;
			display.transform.localPosition = lastPos + Vector3.right * curShake;
		}
		display.transform.localPosition = lastPos;

	}
	IEnumerator SpinDialsToInitial()
    {
		for (var x = 0; x < dialLetters.Length; x++)
			dialLetters[x].text = "";
		var loopsMade = 0;
		while (Enumerable.Range(0, 8).Any(a => assignedDialRotations[a] != curDialRotations[a]) || loopsMade < 10)
		{
			for (var x = 0; x < curDialRotations.Length; x++)
			{
				if (curDialRotations[x] != assignedDialRotations[x])
				{
					curDialRotations[x] = (curDialRotations[x] + 1) % maxDialRotations[x];
					if (curDialRotations[x] == assignedDialRotations[x])
					{
						mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, dials[x]);
					}
					dials[x].localEulerAngles = initialDialRotations[x] + dialRotationSets[x] * curDialRotations[x];
				}
				else
					dialLetters[x].text = encryptedDisplay[x].ToString();
			}
			loopsMade++;
			yield return new WaitForSeconds(loopsMade < 7 ? 0.2f : 0.1f);
		}
		for (var x = 0; x < curDialRotations.Length; x++)
		{
			dialLetters[x].text = encryptedDisplay[x].ToString();
		}
		allowInteractions = true;
		display.text = "";
		display.color = Color.white;
		yield break;
    }
	IEnumerator SpinDialsToSolve()
    {
		display.color = Color.green;
		var loopsMade = 0;
		while (curDialRotations.Any(a => a != 0) || loopsMade < 10)
        {
			for (var x = 0; x < curDialRotations.Length; x++)
			{
				if (curDialRotations[x] != 0)
                {
					curDialRotations[x] = (curDialRotations[x] + 1) % maxDialRotations[x];
					if (curDialRotations[x] == 0)
					{
						mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, dials[x]);
						dialLetters[x].text = disarmText[x].ToString();
					}
					dials[x].localEulerAngles = initialDialRotations[x] + dialRotationSets[x] * curDialRotations[x];
				}
				else
					dialLetters[x].text = disarmText[x].ToString();
			}
			loopsMade++;
			yield return new WaitForSeconds(loopsMade < 7 ? 0.2f : 0.1f);
		}
		for (var x = 0; x < curDialRotations.Length; x++)
		{
			dialLetters[x].text = disarmText[x].ToString();
		}
		display.text = "";
		modSelf.HandlePass();
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		yield break;
    }

	

	void UpdateKeys()
    {
        for (var x = 0; x < letterSelectables.Length; x++)
        {
			letterSelectables[x].transform.localPosition = keySelectableInitialPos.ElementAt(x) + (x == pressedLetterIdx ? 3 : 0) * Vector3.down;
		}
    }

	// TP Handling

	IEnumerator TwitchHandleForcedSolve()
    {
		if (isSolving) yield break;
		autoSolveEnforced = true;
		QuickLog("Requested autosolve viva TP! Disabling strike mechanic.");
		while (!allowInteractions)
			yield return true;
		var idxStart = 0;
		bool conflicting = false;
		for (var x = 0; x < display.text.Length && !conflicting; x++)
        {
			if (expectedResponse[x] != display.text[x]) conflicting = true;
			else idxStart++;
        }
		if (conflicting)
        {
			idxStart = 0;
			letterSelectables.Last().OnInteract();
			yield return new WaitForSeconds(0.05f);
			letterSelectables.Last().OnInteractEnded();
			yield return new WaitForSeconds(0.05f);
		}
		if (string.IsNullOrEmpty(expectedResponse))
		{
			var selectedLetter = letterSelectables.PickRandom();
			selectedLetter.OnInteract();
			yield return new WaitForSeconds(0.05f);
			selectedLetter.OnInteractEnded();
			yield return new WaitForSeconds(0.05f);
		}
		else
		{
            for (var x = idxStart; x < expectedResponse.Length && allowInteractions; x++)
            {
				var curLetter = letterSelectables[keyboardLayout.IndexOf(expectedResponse[x])];
				curLetter.OnInteract();
				yield return new WaitForSeconds(0.05f);
				curLetter.OnInteractEnded();
				yield return new WaitForSeconds(0.05f);
			}
		}
		while (isSolving) yield return true;
		yield break;
    }

#pragma warning disable 414
	private string TwitchHelpMessage = "\"!{0} <A-Z>\" (Example: \"!{0} GREATCMD\") [Inputs those letters, spaces can be used to separate each character] | \"!{0} cancel/delete/clear\" [Deletes inputs], \"!{0} submit/type/enter/input ALUMITES\" [Clears the display, and then submit \"ALUMITES\" on the module]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string cmd)
    {
		if (isSolving)
        {
			yield return "sendtochat {0}, I don't think it's worth to type random stuff here when it is solving or solved already. It won't even show up anyway.";
			yield break;
        }
		Match matchDelete = Regex.Match(cmd, @"^(delete|clear|cancel)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
			matchType = Regex.Match(cmd, @"^(type|enter|input|submit)\s", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		if (matchDelete.Success)
        {
			yield return null;
			letterSelectables.Last().OnInteract();
			yield return new WaitForSeconds(0.05f);
			letterSelectables.Last().OnInteractEnded();
			if (isSolving)
				yield return "solve";
			yield break;
		}
		else if (matchType.Success)
        {
			var matchedValue = matchType.Value.ToUpperInvariant().Split().Skip(1);

			var selectablesToPress = new List<KMSelectable>();
			foreach (string stringSet in matchedValue)
			{
				foreach (char aChar in stringSet)
				{
					var idx = keyboardLayout.IndexOf(aChar);
					if (idx == -1)
					{
						yield return string.Format("sendtochaterror I cannot type the following character onto the module: {0}", aChar);
						yield break;
					}
					selectablesToPress.Add(letterSelectables[idx]);
				}
			}
			if (selectablesToPress.Count != 8)
            {
				yield return string.Format("sendtochaterror I want exactly 8 letters to submit on the module when using this command, no more, no less.");
				yield break;
			}
			yield return null;
			if (display.text.Any())
			{
				letterSelectables.Last().OnInteract();
				yield return new WaitForSeconds(0.05f);
				letterSelectables.Last().OnInteractEnded();
			}
			foreach (KMSelectable curSelectable in selectablesToPress)
			{
				yield return new WaitForSeconds(0.05f);
				curSelectable.OnInteract();
				yield return new WaitForSeconds(0.05f);
				curSelectable.OnInteractEnded();
				if (isSolving)
				{
					yield return "solve";
					yield break;
				}
				else if (!allowInteractions)
					yield break;
			}
		}
		else
        {
			var selectablesToPress = new List<KMSelectable>();
			foreach(string stringSet in cmd.ToUpperInvariant().Split())
            {
				foreach (char aChar in stringSet)
				{
					var idx = keyboardLayout.IndexOf(aChar);
					if (idx == -1)
					{
						yield return string.Format("sendtochaterror I cannot type the following character onto the module: {0}", aChar);
						yield break;
					}
					selectablesToPress.Add(letterSelectables[idx]);
				}
			}
			foreach (KMSelectable curSelectable in selectablesToPress)
            {
				yield return null;
				curSelectable.OnInteract();
				yield return new WaitForSeconds(0.05f);
				curSelectable.OnInteractEnded();
				if (isSolving)
				{
					yield return "solve";
					yield break;
				}
				else if (!allowInteractions)
					yield break;
				yield return new WaitForSeconds(0.05f);
			}
        }
    }
}

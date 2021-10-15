using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RandomStringGenerator : MonoBehaviour {

	public string baseString;

	// Use this for initialization
	void Start () {
		// Scramble a random 26 letter array, no pairs rule in mind.
		Debug.Log(baseString.ToCharArray().Shuffle().Join(""));
		// Assign a random pair to each letter, avoiding overlaps.
		List<char> remainingCharacters = baseString.ToCharArray().ToList();
		string output = "";
		while (remainingCharacters.Any())
        {
			var randomPairSelected = Enumerable.Range(0, remainingCharacters.Count).ToArray().Shuffle().Take(2);
			var selectedCharacters = randomPairSelected.Select(a => remainingCharacters[a]);

			output += selectedCharacters.Join("") + ' ';
			remainingCharacters.RemoveAll(a => selectedCharacters.Contains(a));
		}
		Debug.Log(output.Trim());
	}
}

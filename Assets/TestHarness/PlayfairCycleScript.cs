using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PlayfairCycleScript : MonoBehaviour
{

    public KMAudio Audio;
    public List<KMSelectable> keys;
    public GameObject[] dials;
    public TextMesh[] dialText;
    public TextMesh disp;

    private int r;
    private string[] message = new string[100] { "ADVANCED", "ADVOCATE", "ALLOTYPE", "ALLOTTED", "BINARIES", "BINOMIAL", "BULLHORN", "BULWARKS", "CIRCULAR", "CIRCUITS", "COMMANDO", "COMPILER", "DECRYPTS", "DECIMALS", "DISPATCH", "DISCRETE", "ENCIPHER", "ENCODING", "EQUATORS", "EQUALISE", "FINISHED", "FINNICKY", "FORMULAE", "FORTUNES", "GAUNTLET", "GAUCHEST", "GATEPOST", "GATEWAYS", "HOTLINKS", "HOTHEADS", "HUNTRESS", "HUNGRIER", "INDICATE", "INDIRECT", "ILLUSORY", "ILLUDING", "JIGSAWED", "JIGGLING", "JUNKYARD", "JUNCTURE", "KILOWATT", "KILOBYTE", "KNOCKING", "KNOWABLE", "LIMITING", "LIMERICK", "LINKWORK", "LINGERED", "MONOGRAM", "MONOLITH", "MULTITON", "MULCTING", "NANOGRAM", "NANOBOTS", "NUMEROUS", "NUMERATE", "OCTANGLE", "OCTONARY", "OBSTRUCT", "OBSTACLE", "PROGRESS", "PROGRAMS", "POSTSYNC", "POSITRON", "QUOTIENT", "QUOTABLE", "QUIRKISH", "QUITTERS", "REVERSED", "REVEALED", "ROTATORS", "RELATIVE", "STANZAIC", "STANDARD", "STOCKADE", "STOCCATA", "TRIGGERS", "TRICKIER", "TOMOGRAM", "TOMAHAWK", "UNDERRUN", "UNDOINGS", "ULTERIOR", "ULTRAHOT", "VICINITY", "VICENARY", "VOLITION", "VOLUMING", "WINGDING", "WINGSPAN", "WHATNESS", "WHATSITS", "YEARLONG", "YEASAYER", "YOKOZUNA", "YOURSELF", "ZIGGURAT", "ZIGZAGGY", "ZYMOLOGY", "ZYMOGENE" };
    private string[] response = new string[100] { "DECIMALS", "ULTRAHOT", "ADVANCED", "ILLUSORY", "JUNCTURE", "LIMERICK", "STANDARD", "EQUATORS", "TRICKIER", "ALLOTTED", "PROGRAMS", "PROGRESS", "YEARLONG", "YOKOZUNA", "KILOBYTE", "BINOMIAL", "QUOTIENT", "STOCKADE", "MONOGRAM", "VOLUMING", "OCTANGLE", "WINGSPAN", "OBSTRUCT", "HUNGRIER", "ZIGGURAT", "KILOWATT", "STANZAIC", "BULWARKS", "FORTUNES", "REVERSED", "STOCCATA", "POSITRON", "WHATSITS", "DISCRETE", "OCTONARY", "UNDERRUN", "GAUCHEST", "JUNKYARD", "MULCTING", "QUOTABLE", "ENCIPHER", "BULLHORN", "ZIGZAGGY", "JIGGLING", "ENCODING", "BINARIES", "RELATIVE", "VICINITY", "YEASAYER", "HUNTRESS", "ROTATORS", "NANOBOTS", "INDIRECT", "TOMOGRAM", "OBSTACLE", "LINKWORK", "ZYMOGENE", "REVEALED", "CIRCUITS", "ZYMOLOGY", "COMMANDO", "JIGSAWED", "VOLITION", "WINGDING", "NANOGRAM", "ILLUDING", "UNDOINGS", "KNOCKING", "TOMAHAWK", "ULTERIOR", "CIRCULAR", "QUITTERS", "ADVOCATE", "EQUALISE", "VICENARY", "QUIRKISH", "DECRYPTS", "FORMULAE", "TRIGGERS", "LINGERED", "FINNICKY", "ALLOTYPE", "FINISHED", "HOTHEADS", "POSTSYNC", "YOURSELF", "MONOLITH", "COMPILER", "HOTLINKS", "WHATNESS", "GATEPOST", "NUMERATE", "MULTITON", "INDICATE", "GATEWAYS", "KNOWABLE", "NUMEROUS", "DISPATCH", "LIMITING", "GAUNTLET" };
    private string[] ciphertext = new string[2];
    private string answer;
    private int[][] rot = new int[2][] { new int[8], new int[8] };
    private string[][] digraphs = new string[2][] { new string[4], new string[4] };
    private int pressCount;
    private bool moduleSolved;

    //Logging
    static int moduleCounter = 1;
    int moduleID;

    private void Awake()
    {
        moduleID = moduleCounter++;
        foreach (KMSelectable key in keys)
        {
            int k = keys.IndexOf(key);
            key.OnInteract += delegate () { KeyPress(k); return false; };
        }
    }

    void Start()
    {
        Reset();
    }

    private void KeyPress(int k)
    {
        keys[k].AddInteractionPunch(0.125f);
        if (moduleSolved == false)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            if (k == 25)
            {
                pressCount = 0;
                answer = string.Empty;
            }
            else
            {
                pressCount++;
                answer = answer + "QWERTYUIOPASDFGHJKLZCVBNM"[k];
            }
            disp.text = answer;
            if (pressCount == 8)
            {
                if (answer == ciphertext[1])
                {
                    moduleSolved = true;
                    Audio.PlaySoundAtTransform("InputCorrect", transform);
                    disp.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    disp.color = new Color32(255, 0, 0, 255);
                    Debug.LogFormat("[Playfair Cycle #{0}]The submitted response was {1}: Resetting", moduleID, answer);
                }
                Reset();
            }
        }
    }

    private void Reset()
    {

        StopAllCoroutines();
        if (moduleSolved == false)
        {
            pressCount = 0;
            answer = string.Empty;
            r = Random.Range(0, 100);
            string[] roh = new string[8];
            List<string>[] keyword = new List<string>[2] { new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "Y", "Z" }, new List<string> { } };
            string[][] keytable = new string[5][] { new string[5], new string[5], new string[5], new string[5], new string[5] };
            List<string>[] ciph = new List<string>[] { new List<string> { }, new List<string> { } };
            for (int i = 0; i < 8; i++)
            {
                dialText[i].text = string.Empty;
                rot[1][i] = rot[0][i];
                rot[0][i] = Random.Range(0, 5);
            }
            roh[0] = rot[0][0].ToString();
            int[] keynum = new int[7];
            List<int> usedvals = new List<int> { };
            for (int i = 1; i < 8; i++)
            {
                keynum[i - 1] = 5 * rot[0][i - 1] + rot[0][i];
                while (usedvals.Contains(keynum[i - 1]))
                {
                    rot[0][i] = Random.Range(0, 5);
                    keynum[i - 1] = 5 * rot[0][i - 1] + rot[0][i];
                }
                roh[i] = rot[0][i].ToString();
                usedvals.Add(keynum[i - 1]);
                keyword[1].Add(keyword[0][keynum[i - 1]]);
            }
            for (int i = 0; i < 25; i++)
            {
                if (i < 7)
                {
                    keyword[0].Remove(keyword[1][i]);
                }
                else
                {
                    keyword[1].Add(keyword[0][i - 7]);
                }
                keytable[Mathf.FloorToInt(i / 5)][i % 5] = keyword[1][i];
            }           
            for(int i = 0; i < 4; i++)
            {                
                if(message[r][2 * i] == message[r][2 * i + 1])
                {
                    digraphs[0][i] = message[r][2 * i].ToString() + "Z";
                }
                else
                {
                    digraphs[0][i] = message[r][2 * i].ToString() + message[r][2 * i + 1].ToString();
                }
                if (response[r][2 * i] == response[r][2 * i + 1])
                {
                    digraphs[1][i] = response[r][2 * i].ToString() + "Z";
                }
                else
                {
                    digraphs[1][i] = response[r][2 * i].ToString() + response[r][2 * i + 1].ToString();
                }
            }
            for(int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int[] y = new int[2] { keyword[1].IndexOf(digraphs[j][i][0].ToString()) % 5, keyword[1].IndexOf(digraphs[j][i][1].ToString()) % 5 };
                    int[] x = new int[2] { Mathf.FloorToInt(keyword[1].IndexOf(digraphs[j][i][0].ToString()) / 5), Mathf.FloorToInt(keyword[1].IndexOf(digraphs[j][i][1].ToString()) / 5) };
                    if (x[0] == x[1])
                    {
                        ciph[j].Add(keytable[x[0]][(y[0] + 1) % 5]);
                        ciph[j].Add(keytable[x[1]][(y[1] + 1) % 5]);
                    }
                    else if(y[0] == y[1])
                    {
                        ciph[j].Add(keytable[(x[0] + 1) % 5][y[0]]);
                        ciph[j].Add(keytable[(x[1] + 1) % 5][y[1]]);
                    }
                    else
                    {
                        ciph[j].Add(keytable[x[0]][y[1]]);
                        ciph[j].Add(keytable[x[1]][y[0]]);
                    }
                }
            }
            ciphertext[0] = string.Join(string.Empty, ciph[0].ToArray());
            ciphertext[1] = string.Join(string.Empty, ciph[1].ToArray());
            Debug.LogFormat("[Playfair Cycle #{0}]The encrypted message was {1}", moduleID, ciphertext[0]);
            Debug.LogFormat("[Playfair Cycle #{0}]The dial rotations were {1}", moduleID, string.Join(", ", roh));
            string logsquare;
            for(int i = 0; i < 5; i++)
            {
                if (i == 0)
                    logsquare = "The keysquare was:\n[Playfair Cycle #" + moduleID +"]";
                else
                    logsquare = string.Empty;
                Debug.LogFormat("[Playfair Cycle #{0}] {2} {1}", moduleID, string.Join(" ", keytable[i]), logsquare);
            }
            Debug.LogFormat("[Playfair Cycle #{0}]The deciphered message was {1}", moduleID, message[r]);
            Debug.LogFormat("[Playfair Cycle #{0}]The response word was {1}", moduleID, response[r]);
            Debug.LogFormat("[Playfair Cycle #{0}]The correct response was {1}", moduleID, ciphertext[1]);
        }
        StartCoroutine(DialSet());
    }

    private IEnumerator DialSet()
    {
        int[] spin = new int[8];
        bool[] set = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            if (moduleSolved == false)
            {
                spin[i] = rot[0][i] - rot[1][i];
            }
            else
            {
                spin[i] = -rot[0][i];
            }
            if (spin[i] < 0)
            {
                spin[i] += 5;
            }
        }
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (spin[j] == 0)
                {
                    if (set[j] == false)
                    {
                        set[j] = true;
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
                        if (moduleSolved == false)
                        {
                            dialText[j].text = ciphertext[0][j].ToString();
                        }
                        else
                        {
                            switch (j)
                            {
                                case 0:
                                    dialText[j].text = "W";
                                    break;
                                case 2:
                                case 3:
                                    dialText[j].text = "L";
                                    break;
                                case 4:
                                    dialText[j].text = "D";
                                    break;
                                case 5:
                                    dialText[j].text = "O";
                                    break;
                                case 6:
                                    dialText[j].text = "N";
                                    break;
                                default:
                                    dialText[j].text = "E";
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    dials[j].transform.localEulerAngles += new Vector3(0, 0, 72);
                    spin[j]--;
                }
            }
            if (i < 7)
            {
                yield return new WaitForSeconds(0.4f);
            }
        }
        if (moduleSolved == true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            GetComponent<KMBombModule>().HandlePass();
        }
        disp.text = string.Empty;
        disp.color = new Color32(255, 255, 255, 255);
        yield return null;
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "!{0} <A-Z> [Inputs letters] | !{0} cancel [Deletes inputs]";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {

        if (command.ToLowerInvariant() == "cancel")
        {
            yield return null;
            keys[26].OnInteract();
        }
        else
        {
            command = command.ToUpperInvariant();
            var word = Regex.Match(command, @"^\s*([A-WYZ]+)\s*$");
            if (!word.Success)
            {
                yield return "sendtochaterror \"" + command + "\" is an invalid command";
                yield break;
            }
            yield return null;
            command = command.Replace(" ", string.Empty);
            foreach (char letter in command)
            {
                keys["QWERTYUIOPASDFGHJKLZCVBNM".IndexOf(letter)].OnInteract();
                yield return new WaitForSeconds(0.125f);
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Video;
using Rnd = UnityEngine.Random;

public class HexOS : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo Info;
    public KMBombModule Module;
    public KMSelectable Button;
    public TextMesh Top;
    public TextMesh Bottom;
    public TextMesh BigThing;
    public AudioSource Sounds;
    public AudioClip[] Holdsounds;
    bool _isHolding;
    bool Ready;
    int PressCount;
    int dashordot;
    int WhatToSubmit;
    string Submission;
    int Submitting;
    string[] MorseCode =
    {".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.."};
    private static int _moduleIdCounter = 1;
    private int _moduleId = 0;
    private bool moduleSolved;

    private void Awake()
    {
        Submission = "";
        WhatToSubmit = Rnd.Range(0, 26);
        StartCoroutine(StartTheThing());
        Button.OnInteract += delegate ()
        {
            HandlePress();
            return false;
        };
        Button.OnInteractEnded += delegate ()
        {
            HandleRelease();
        };
        Debug.LogFormat("[h #{0}] Submit {1} to solve the module.", _moduleId, MorseCode[WhatToSubmit]);
    }

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
    }
    
    private void HandlePress()
    {
        if (Ready)
        {
            Button.AddInteractionPunch(0.1f);
            Submitting = 0;
            _isHolding = true;
            Sounds.clip = Holdsounds[0];
            Sounds.Play();
        }
    }
    private void HandleRelease()
    {
        if (_isHolding)
        {
            _isHolding = false;
            PressCount++;
            if (dashordot < 50)
                Submission = Submission + ".";
            else if (dashordot >= 50 && dashordot < 125)
                Submission = Submission + "-";
            else if (dashordot >= 150)
            {
                Submission = "";
                PressCount = 0;
            }
            dashordot = 0;
        }
    }
    private void FixedUpdate()
    {
        if (_isHolding && !moduleSolved)
        {
            dashordot++;
        }
        if (dashordot == 50)
        {
            Sounds.clip = Holdsounds[1];
            Sounds.Play();
        }
        if (dashordot == 150)
        {
            Sounds.clip = Holdsounds[2];
            Sounds.Play();
        }
        else if (!_isHolding && !moduleSolved && Submission != "")
        {
            Submitting++;
        }
        if (Submitting == 100)
        {
            Debug.LogFormat("[h #{0}] You submitted {1} which is...", _moduleId, Submission);
            if (Submission == MorseCode[WhatToSubmit])
            {
                Debug.LogFormat("[h #{0}] correct! Congrats, you solved this dumb idea of a module I had.", _moduleId);
                moduleSolved = true;
                Submitting = 0;
                Module.HandlePass();
                Sounds.clip = Holdsounds[4];
                Sounds.Play();
                Top.text = "";
                Bottom.text = "(Get it?)";
                BigThing.characterSize = 0.5f;
                BigThing.text = "HH";
            }
            else
            {
                Debug.LogFormat("[h #{0}] wrong. Sorry. Strike. Feel lucky I didn't make this thing reset upon a strike.", _moduleId);
                if (PressCount > 5)
                    Debug.LogFormat("[h #{0}] Wait, that's more dots/dashes than every morse letter! Why?!", _moduleId);
                Module.HandleStrike();
                Ready = false;
                Submitting = 0;
                PressCount = 0;
                Submission = "";
                StartCoroutine(YouFailed());
            }
        }
    }
    IEnumerator YouFailed()
    {
        yield return new WaitForSeconds(0.01f);
        Top.text = "";
        Bottom.text = "";
        BigThing.characterSize = 0.5f;
        BigThing.text = ":(";
        StartCoroutine(StartTheThing());
    }
    IEnumerator StartTheThing()
    {
        yield return new WaitForSeconds(2f);
        BigThing.text = "H";
        BigThing.characterSize = 1;
        for (int i = 0; i < WhatToSubmit + 1; i++)
        {
            Sounds.PlayOneShot(Holdsounds[3], 1.0f);
            if (i > 12)
            {
                Bottom.text += "H";
                if (i % 4 == 0)
                    Bottom.text += System.Environment.NewLine;
            }
            else
            {
                Top.text += "H";
                if (i % 4 == 3)
                    Top.text += System.Environment.NewLine;
            }
            yield return new WaitForSeconds(0.07f);
        }
        Ready = true;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"'!{0} submit [submission]' will submit the given morse sequence. (Replace [submission] with a series of - and .)";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] TheStuff = command.Split(' ');
        if (Regex.IsMatch(TheStuff[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (TheStuff.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
                yield break;
            }
            else if (TheStuff.Length == 1)
            {
                yield return "sendtochaterror I need something to submit!";
                yield break;
            }
            else if (TheStuff[1] != ".-" && TheStuff[1] != "-..." && TheStuff[1] != "-.-." && TheStuff[1] != "-.." && TheStuff[1] != "." && TheStuff[1] != "..-." && TheStuff[1] != "--." && TheStuff[1] != "...." && TheStuff[1] != ".." && TheStuff[1] != ".---" && TheStuff[1] != "-.-" && TheStuff[1] != ".-.." && TheStuff[1] != "--" && TheStuff[1] != "-." && TheStuff[1] != "---" && TheStuff[1] != ".--." && TheStuff[1] != "--.-" && TheStuff[1] != ".-." && TheStuff[1] != "..." && TheStuff[1] != "-" && TheStuff[1] != "..-" && TheStuff[1] != "...-" && TheStuff[1] != ".--" && TheStuff[1] != "-..-" && TheStuff[1] != "-.--" && TheStuff[1] != "--..")
            {
                yield return "sendtochaterror This is not a valid morse letter!";
                yield break;
            }
            else
            {
                if (TheStuff[1] == MorseCode[WhatToSubmit])
                {
                    moduleSolved = true;
                    Submitting = 0;
                    Module.HandlePass();
                    Sounds.clip = Holdsounds[4];
                    Sounds.Play();
                    Top.text = "";
                    Bottom.text = "ok cool";
                    BigThing.characterSize = 0.5f;
                    BigThing.text = "AY";
                    Debug.LogFormat("[h #{0}] Twitch Plays has solved the module, nice job I guess", _moduleId);
                }
                else
                {
                    StartCoroutine(YouFailed());
                    Debug.LogFormat("[h #{0}] TP-side strike detected!", _moduleId);
                }
            }
        }
    }
}
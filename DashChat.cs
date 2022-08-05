using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class Option
{
    public string text;
    public int index;
}

public class DashChat : MonoBehaviour
{
    public TextAsset tempText; //TODO Delete

    [System.Serializable]
    public enum State { start, readyNext, processing, awaiting, end };
    public State chatState = State.start;
    protected private string[] dialogueLines;

  

    public List<Option> currentOptions = new List<Option>();
    private int chatIndex;

    public int depth = 0;
    public int lastDepth = 0;
    //Cache regex for performance
    private static Regex regexOptions = new Regex("[0-9]. ", RegexOptions.Compiled);
    private static Regex regexQuestion = new Regex("# ", RegexOptions.Compiled);
    private static Regex regexFormater = new Regex("- ", RegexOptions.Compiled);
    private static Regex regexEnd = new Regex("<end>", RegexOptions.Compiled);
    private static Regex regexTrimmer = new Regex(@"^\s+", RegexOptions.Compiled);

    public static DashChat dash;
    //Initialized the Dialogue manager singleton
    public static DashChat instance
    {
        get
        {
            if (!dash)
            {
                dash = FindObjectOfType(typeof(DashChat)) as DashChat;
                if (!dash)
                {
                    Debug.LogError("You need at least 1 active DashChat script");
                }
            }
            return dash;
        }
    }
    // Start is called before the first frame update
    void Awake()
    {
        dash = GetComponent<DashChat>();
        depth = 0;
        InitializeChat(tempText); //TODO Delete
    }

    // Start Conversation
    public void InitializeChat(TextAsset _chatFile)
    {
        onProceedChat?.Invoke();
        depth = 0; //Set start of convo
        chatIndex = 0; // Start from the first line.
        //splits the entire convo up
        dialogueLines = _chatFile.text.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
        //Check to see if there are lines
        if (dialogueLines.Length == 0)
        {
            Debug.LogWarning("Not a valid text file");
            return;
        }
        ReadLine();
    }

    public async void ReadLine()
    {
        string _line = dialogueLines[chatIndex]; //get line of text
        string _type = "none";
        bool _valid = validLineOfText(_line);
        int _lineDepth = getDepth(_line);
        if (_lineDepth == depth && _valid) //check if line is in current depth
        {
            _line = Prepare(_line); //Formats line
            _type = ProcessLine(_line); //Processes line.
        }
        else if (!_valid)
        {
            NextLine(); // skip to the next line.
        }
        //Waits for the processing to finish before proceeding to next line
        while (chatState == State.processing || chatState == State.awaiting)
        {
            if (chatState == State.processing)
            {
                Debug.Log("Processing");
            }
            else
            {
                Debug.Log("Awaiting");
            }
            await Task.Yield(); //yeilds code from running before reading next line
        }
        // Checks to see if the last line was the end of a thread the logic is if the current convo is in a thread any
        // any line that proceeds is not the same depth it must be a new thread.
        // i.e.
        // - convo at a depth of 1
        // - - Convo at a depth of 2

        //if (_lineDepth == depth && _type!="question")
        //{
        //    lastDepth = _lineDepth;
        //    NextLine();
        //}

    }

    public void NextLine()
    {
        if (chatIndex < dialogueLines.Length - 1 && chatState == State.readyNext)
        {
            chatIndex++;
            ReadLine();
        }
    }
    string Prepare(string ReadText)
    {
        string prepString = regexFormater.Replace(ReadText, string.Empty);//Removes '- ' from text
        prepString = regexTrimmer.Replace(prepString, "");
        return prepString;
    }
    private bool validLineOfText(string _line)
    {
        char[] _characters = _line.ToCharArray();
        if (_characters.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int getDepth(string _line)
    {
        return Regex.Matches(_line, "- ").Count; //Checks depth of line
    }
    private void SetDepth(string _line)
    {
        depth = Regex.Matches(_line, "- ").Count;
    }

    private string ProcessLine(string _line)
    {
        chatState = State.processing;
        if (regexEnd.IsMatch(_line))
        {
            onEndChat?.Invoke();
            chatState = State.end;
            return "end";
        }
        else
        {
            if (regexQuestion.IsMatch(_line)) // Check to see if it is a question
            {
                currentOptions.Clear(); //clear out previous options
                Debug.Log("Question:" + _line); // TODO Delete
                DisplayChat(_line);
                //Retrieve options
                for (int i = chatIndex + 1; i < dialogueLines.Length; i++) //loop through convo starting on the next line
                {
                    string _text = dialogueLines[i];
                    if (getDepth(_text) == depth) //check if line is in current depth
                    {
                        if (regexOptions.IsMatch(_text)) //check to see if its an option
                        {
                            Option op = new Option();
                            op.text = _text;
                            op.index = i;
                            currentOptions.Add(op);
                        }
                        else // if its not an option there all are assumed to be found.
                        {
                            i = dialogueLines.Length; // stop loop
                        }
                    }
                }
                if (currentOptions.Count > 0)
                {
                    chatState = State.awaiting;
                    DisplayOptions(currentOptions); // Displays current options
                }
                return "question";
            }
            else
            {
                Debug.Log("Text:" + _line);
                DisplayChat(_line);
                chatState = State.readyNext;
                return "text";
            }
        }

    }

    /// <summary>
    /// Selects option by passing the matching key and progresses conversation in new thread.
    /// </summary>
    /// <param name="_key"></param>
    public void SelectOptions(int _key)
    {
        chatIndex = currentOptions[_key].index + 1; // Set chat index to next line after option
        SetDepth(dialogueLines[chatIndex]); // Set Depth to new line
        lastDepth = getDepth(dialogueLines[chatIndex]);
        chatState = State.readyNext; // Set Chat state to ready next
        ReadLine();
    }


    // Actions
    public static event Action onProceedChat;
    public static event Action onEndChat;
    public static event Action<char[]> onDisplayChat;
    public static event Action<List<Option>> onDisplayOptions;

    public void DisplayChat(string _line)
    {
        onDisplayChat?.Invoke(_line.ToCharArray());
    }

    public void DisplayOptions(List<Option> _options)
    {
        onDisplayOptions?.Invoke(_options);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class Option
{
    public string text;
    public int index;
}
[System.Serializable]
public enum DSState { start, readyNext, processing, awaiting, end };

public class DashChat : MonoBehaviour
{
    public TextAsset tempText; //TODO Delete


    public DSState chatState = DSState.start;
    protected private string[] dialogueLines;



    public List<Option> currentOptions = new List<Option>();
    public static string currentVariable;
    public int chatIndex;

    public int depth = 0;

    //Cache regex for performance
    private static Regex regexOptions = new Regex("[0-9]. ", RegexOptions.Compiled);
    private static Regex regexQuestion = new Regex("# ", RegexOptions.Compiled);
    private static Regex regexFormater = new Regex("- ", RegexOptions.Compiled);
    private static Regex regexEnd = new Regex("<end>", RegexOptions.Compiled);
    private static Regex regexTrimmer = new Regex(@"^\s+", RegexOptions.Compiled);
    private static Regex regexJump = new Regex("<jump.*?>", RegexOptions.Compiled);
    private static Regex regexVariable = new Regex("<(variable.*?)>", RegexOptions.Compiled);
    private static Regex regexEvent = new Regex("<event.*?>", RegexOptions.Compiled);
    private static Regex regexEmotion = new Regex("<emotion.*?>", RegexOptions.Compiled);
    private static Regex regexSpeed = new Regex("<speed.*?>", RegexOptions.Compiled);

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

   /// <summary>
   /// Initializes conversations by loading a text file
   /// </summary>
   /// <param name="_chatFile"></param>
    public void InitializeChat(TextAsset _chatFile)
    {
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

    /// <summary>
    /// Reads the line of text in the text file
    /// </summary>
    public void ReadLine()
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
    }

    /// <summary>
    /// Handles reading the next line if ready to and another line is waiting to be read.
    /// </summary>
    public void NextLine()
    {
        if (chatIndex < dialogueLines.Length - 1 && chatState == DSState.readyNext)
        {
            chatIndex++;
            OnNextLine?.Invoke(); // trigger event a new line being read
            ReadLine();
        }
    }

    /// <summary>
    /// Prepares string by removing formatting characters/syntax and replaces variables with actual values
    /// </summary>
    /// <param name="ReadText"></param>
    /// <returns></returns>
    string Prepare(string ReadText)
    {
        string prepString = regexFormater.Replace(ReadText, string.Empty);//Removes '- ' from text
        prepString = regexVariable.IsMatch(prepString) ? HandleVariable(prepString) : prepString;
        prepString = regexTrimmer.Replace(prepString, "");
        return prepString;
    }


    /// <summary>
    /// Verifies if the current line is valid and not blank.
    /// </summary>
    /// <param name="_line"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Fetches the depth of the line 
    /// </summary>
    /// <param name="_line"></param>
    /// <returns></returns>
    private int getDepth(string _line)
    {
        return Regex.Matches(_line, "- ").Count; //Checks depth of line
    }

    /// <summary>
    /// Sets the depth of the line
    /// </summary>
    /// <param name="_line"></param>
    private void SetDepth(string _line)
    {
        depth = Regex.Matches(_line, "- ").Count;
    }

    /// <summary>
    /// Processes line based on its type a question, jump, event, text, or the end of the convo. 
    /// </summary>
    /// <param name="_line"></param>
    /// <returns></returns>
    private string ProcessLine(string _line)
    {
        ChangeState(DSState.processing);
        if (regexEnd.IsMatch(_line))
        {
            OnEndChat?.Invoke();
            ChangeState(DSState.end);
            return "end";
        }
        else if (regexEvent.IsMatch(_line))
        {
            HandleEvent(_line);
            return "event type";
          }
        else if (regexJump.IsMatch(_line))
        {
            int _jumpTo = int.Parse(_line.Substring(6, _line.Length - 7));
            SetDepth(dialogueLines[_jumpTo]);
            chatIndex = _jumpTo - 2;
            ChangeState(DSState.readyNext);
            Debug.Log("jumping to" + _line.Substring(6, _line.Length - 7));
            NextLine();
            return "jumping to" + _line.Substring(6, _line.Length - 7);
        }
        else
        {
            if (regexQuestion.IsMatch(_line)) // Check to see if it is a question
            {
                currentOptions.Clear(); //clear out previous options
                Debug.Log("Question:" + _line); // TODO Delete
                DisplayChat(regexQuestion.Replace(_line, ""));
                //Retrieve options
                for (int i = chatIndex + 1; i < dialogueLines.Length; i++) //loop through convo starting on the next line
                {
                    string _text = dialogueLines[i];
                    if (getDepth(_text) == depth) //check if line is in current depth
                    {
                        if (regexOptions.IsMatch(_text)) //check to see if its an option
                        {
                            Option op = new Option();

                            op.text = regexOptions.Replace(_text, "");
                            op.text = regexFormater.Replace(op.text, "");
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
                    ChangeState(DSState.awaiting);
                    DisplayOptions(currentOptions); // Displays current options
                }
                return "question";
            }
            else
            {
                Debug.Log("Text:" + _line);
                DisplayChat(_line);
                ChangeState(DSState.readyNext);
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
        ChangeState(DSState.readyNext); // Set Chat state to ready next
        ReadLine();
    }


    // Action Events
    /// <summary>
    /// Emits when a new line is being read
    /// </summary>
    public static event Action OnNextLine;

    /// <summary>
    /// Emits when the conversation has been signaled to end
    /// </summary>
    public static event Action OnEndChat;

    /// <summary>
    /// Emits that it ready to display a chat.
    /// </summary>
    public static event Action<string> OnDisplayChat;

    /// <summary>
    /// Emits when options are ready to be displayed
    /// </summary>
    public static event Action<List<Option>> OnDisplayOptions;

    /// <summary>
    /// Emits when there is a change of state for the DashChat
    /// </summary>
    public static event Action<DSState> OnChangedState;

    /// <summary>
    /// Emits when a variable is required to be looked up. 
    /// </summary>
    public static event Action<string> OnLookUpVariable;

    /// <summary>
    /// Emits when a event is to be invoked
    /// </summary>
    public static event Action<string> OnTriggerEvent;


    public void DisplayChat(string _line)
    {
        OnDisplayChat?.Invoke(_line);
    }

    public void DisplayOptions(List<Option> _options)
    {
        OnDisplayOptions?.Invoke(_options);
    }

    public void ChangeState(DSState changeTo)
    {
        chatState = changeTo;
        OnChangedState?.Invoke(changeTo);
    }
    //Handles variables inside of line of text
    string HandleVariable(string _variableKey)
    {
        object match = regexVariable.Match(_variableKey);
        OnLookUpVariable?.Invoke(match.ToString());

        // Awaits for external variable to be provided. 
        while (currentVariable == "")
        {
            // await for variable to be looked up and provided.
        }
        // Look up variable in global variable dictionary scriptable object
        string processString = regexVariable.Replace(_variableKey, currentVariable);
        currentVariable = ""; //reset variable to empty
        return processString;
    }

    //Signals to the event manager to trigger a event. 
    void HandleEvent(string _event)
    {
        object inGameEvent = regexEvent.Match(_event);
        if (inGameEvent != null)
        {
            string eventName = inGameEvent.ToString();
            OnTriggerEvent?.Invoke(eventName.Substring(7, eventName.Length - 8));
        };
        chatState = DSState.readyNext;
        NextLine();
    }
}

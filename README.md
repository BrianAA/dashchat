# Dashchat
A open source dialogue maker for unity. 

## How it works
Dash chat is powered by one script it provides a way to parse a formatted text file to generate a in game dialogue. It uses
" - " dashes to indicate message threads within a conversation allowing for options to be nested and naturally be written out.

## Getting started
To start copy and paste the `dashchat.cs` into your project. Add it to a game object and it is ready to be used. You can use the example text file found in the repo to help you start. 

### Initializing a chat
Generate a text file and use it as the source of the dialogue. Start the dialogue by invoking `DashChat.instance.InitializeChat($your text file)`.

``` c#
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

```


### Reading the next line
Reading the next line in the file can be requested by invoking `DashChat.instance.NextLine()`. Here is an example on how to trigger 
Dashchat to read the next line. Ideally this should be controlled via the ui or an input manager

``` c#
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && DashChat.instance.chatState==DSState.readyNext){ 
            DashChat.instance.NextLine();
        }
    }
```

----

# Formatting & Features

## Dash format
The main formatting of the text file is the dash. This helps DashChat know the current depth of the conversation and not 
worry about lines that are outside of the current thread. When a conversation is moved in to a thread the depth will increase
by one int. 

```
This is the main conversation
# This question is also part of the main convo
1. This response is part of the main convo.
- This is a new thread at a depth of 1. 
- <end>
2. This response is part of the main convo.
- This is a new thread at a depth of 1
- <end>
```


## Variables
Variables work by embedding the following syntax within a line: `<variables.$yourVariable>`.

Create your own provision method and assign it to `DashChat.VariableProvider`.

Example
```
Hello <variable.playerName>!
```

``` C#
DashChat.VariableProvider = (string variableName) => {
    if(variableName == "playerName"){
        return "Player One";
    }
    ...
    else {
        return dict[variableName];
    }
}
```


## Jump lines
The Jump feature allows you to break out of the thread or loop back to a previous line. 
To use it simply add a label to your text file using the following syntax `::$labelName` (Regex `/::\w+/`).
Trigger jumps by adding a `<jump.::$labelName>` line to your file.

It is also possible to jump to a specific line using `<jump.$theLineNumber>` The line number in the text file should be number in the jump.

*Hint:* You can pass through a labeled section without jumping to it. This can be prevented by adding a line containing an `<end>` tag before the label.

Example

``` txt
1 # How are you?
2 1. I am okay
3 - That is good
4 - <jump.::Continuation>
5 2. Meh
6 - Oh man that sucks
7 - <jump.9>
8 ::Continuation
9 Anywho I was wondering something...
```

## Trigger Events

Trigger events by adding an event line like so: `<event.$yourEventName($$)>`
Dashchat will emit the `OnTriggerEvent` to notify your event manager about the request. Variables can be used by adding `()` to the event. 

Example
```
Hello!
# Guess what?
1. What?
- You are now my friend!
- <event.NewFriend(Tommy)>
2. Leave me alone.
- Well then nevermind!
- <event.NewEnemey(Tommy)>
```


## Switch actors
When there is the need for multiple NPCs to be part of a single text file, use the `<switch.$your_actor_name>` tag. The switch tag must occur on its own line. Dashchat will emit the `OnActorSwitch` event to notify that a switch was requested. 

Example
```
Hey there I am Tommy!
<switch.Timmy>
And I am Timmy!
<switch.Tommy>
How can I help you today?
...
```

## Questions and options
To write out question and options simply add the `#` sign to signal a question. 

After defining a question, provide options using this syntax: `1.` (the number and then a period.)

Define the resulting dialog by adding lines with a greater depth between the defined options.

Example
```
# Hello how are you?
1. I am okay
- Oh that is good
2. I been better
- Oh that sucks
```

## Ending the dialogue
End the dialog by adding a line containing the `<end>` tag.

Example
```
Hey how are you?
1. Fine
- That is good
- <end>
2. Meh
- Oh sorry to hear
- <end>
```

Another example
```
This is great! 
I can't imagine a better time to do this. 
So I will get started
<end>
This line of text will not be read. 
```

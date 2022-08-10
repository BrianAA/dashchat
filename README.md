# Dashchat
A open source dialogue maker for unity. 

## How it works
Dash chat is powered by one script it provides a way to parse a formatted text file to generate a in game dialogue. It uses
" - " dashes to indicate message threads within a conversation allowing for options to be nested and naturally be written out.

## Getting started
To start copy and paste the `dashchat.cs` into your project. Add it to a game object and it is ready to be used. You can use the example text file found in the repo to help you start. 

### Initializing a chat
Generate a text file and use it as the source of the dialogue. Calling `DashChat.dash.initialize($your text file)` will start the 
dialogue and Dash chat will being to parse it out. 
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
Reading the next line in the file can be controlled by invoking `NextLine()` in dashchat. Here is an example on how to trigger 
Dashchat to read the next line. Ideally this should be controlled via the ui or a input manager

``` c#
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && DashChat.dash.chatState==DSState.readyNext){ 
            DashChat.dash.NextLine();
        }
    }
```

----

# Formatting & Features

## Dash format
The main formatting of the text file is the dash. This helps DashChat know what is the current depth of the conversation and not 
worry about conversations that are happening within a thread. When a conversation is moved in to a thread the depth will increase
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
Variables work by embedding the following syntax within a line <variables.$your_variable> 
you then can handle that variable in your game and return it back to the DashChat by setting the static variable
`currentVariable` this will then replace the variable in the string and proceed to finish processing the line. 

Eample
```
Hello <variable.playerName>!
```
The event `onLookUpVariable` will emit and notify your variable handler to provide `currentVariable` to DashChat. 

``` C#
DashChat.dash.currentVariable=" Player one"
```


## Jump lines
Jump feature is a feature to allow you to break out of the threads or loop back to a previous line. 
To use it simply add `<jump.$the_line number>` The number in the text file should be number in the jump.

``` txt
1 Hello
2 # How are you?
3 1. I am okay
4 - That is good
5 - <jump.9>
6 2. Meh
7 - Oh man that sucks
8 - <jump.9>
9 Anywho I was wondering something...
```

## Trigger Events
To trigger events the event need to on their own line in the text file. Dashchat will not process the event as text but will 
emit the event to notify to your event manager what event need to fire. Vairables can be used by adding `()` to the event. 
To use simple write <event.$yourEventName($$)>

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
When there is a need for a multiple NPC to be a part of a single text file use the `<switch.$your_actor_name>` feature. The switch feature must occur on its own line. Dashchat will emit the event to notify that a switch is needed. 

```
Hey there I am Tommy!
<switch.Timmy>
And I am Timmy!
<switch.Tommy>
How can I help you today?
...
```

## Questions and options
To write out question and options simple add the `#` sign to signal a question. Then following the question must be all the options
writte using the syntax `1.` (the number and then a period.)

```
# Hello how are you?
1. I am okay
- Oh that is good
2. I been better
- Oh that sucks
```

## Ending the dialogue
To end the dialogue the end needs to be signaled on its own line as `<end>`

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

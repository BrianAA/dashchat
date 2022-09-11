using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quick way to debug/test DashChat changes with minimal setup. Just add this to a GameObject, define demoText and you are set.
/// </summary>
public class DashChatTester : MonoBehaviour
{
    public TextAsset demoText;

    private DashChat dashChat;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            dashChat.NextLine();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            dashChat.SelectOptions(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            dashChat.SelectOptions(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            dashChat.SelectOptions(2);
        }
        //TODO: Add options 3..9?
    }
    private void OnEnable()
    {
        dashChat = DashChat.instance;
        SubscribeDashChatEvents();
        DashChat.VariableProvider = ProvideVariableValue;
        dashChat.InitializeChat(demoText);
    }

    private string ProvideVariableValue(string variableName)
    {
        Output($"variable lookup: {variableName}");

        return $"DemoVariable:{variableName}";
    }

    private void OnDisable()
    {
        UnsubscribeDashChatEvents();
        DashChat.VariableProvider = null;
    }

    private void SubscribeDashChatEvents()
    {
        Output("Subscribing to Events");

        DashChat.OnDisplayChat += DashChat_OnDisplayChat;
        DashChat.OnDisplayOptions += DashChat_OnDisplayOptions;
        DashChat.OnTriggerEvent += DashChat_OnTriggerEvent;
        DashChat.OnChangedState += DashChat_OnChangedState;
        DashChat.OnActorSwitch += DashChat_OnActorSwitch;
    }

    private void UnsubscribeDashChatEvents()
    {
        Output("Unsubscribing Events");

        DashChat.OnDisplayChat -= DashChat_OnDisplayChat;
        DashChat.OnDisplayOptions -= DashChat_OnDisplayOptions;
        DashChat.OnTriggerEvent -= DashChat_OnTriggerEvent;
        DashChat.OnChangedState -= DashChat_OnChangedState;
        DashChat.OnActorSwitch -= DashChat_OnActorSwitch;
    }

    private void DashChat_OnDisplayOptions(List<Option> obj)
    {
        var optionStrings = new List<string>();

        for (int i = 0; i < obj.Count; i++)
        {
            optionStrings.Add($"{i}: {obj[i].text}");
        }

        Output($"options: {string.Join(Environment.NewLine, optionStrings)}");
    }
    private void DashChat_OnDisplayChat(string obj)
    {
        Output($"text: {obj}");
    }

    private void DashChat_OnActorSwitch(string obj)
    {
        Output($"switched actor to: {obj}");
    }

    private void DashChat_OnChangedState(DSState obj)
    {
        Output($"changed state to {obj}");
    }

    private void DashChat_OnTriggerEvent(string obj)
    {
        Output($"triggered custom event: {obj}");
    }

    private void Output(string str)
    {
        Debug.Log($"DashChatTester> {str}");
    }
}
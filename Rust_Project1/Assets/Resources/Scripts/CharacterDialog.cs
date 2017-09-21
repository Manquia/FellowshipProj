using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CharacterDialog : MonoBehaviour
{   
    [Serializable]
    public class Dialog
    {
        [Serializable]
        public struct Condition
        {
            public enum Type
            {
                InParty,
                Area,
                Custom,
            }
            public Type type;
            public bool invertCondition;
            public string identifier;
        }
        [Serializable]
        public struct Echo
        {
            public DialogManager.OratorNames orator;
            public string text;
            public QueuedDialog.Type type;
        }

        //public bool recurring;
        //public bool blocking;
        //public bool interupting;

        public Condition[] condition;
        public Echo[] conversation;
        public string[] sideEffects;
    }

    public Dialog[] dialogSets;
}

using System.Collections.Generic;
using UnityEngine;

// 캐릭터 및 대화
[System.Serializable]
public class CharacterAndConversation
{
    public CharacterInfo characterInfo;
    [TextArea] public string character_conversation_text;
}

[CreateAssetMenu(fileName = "Conversation", menuName = "ScriptableObjects/Conversation")]
public class ConversationSO : ScriptableObject
{
    public List<CharacterAndConversation> CACList;
}

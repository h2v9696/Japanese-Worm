using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
[System.Serializable]
public class Word
{    //these variables are case sensitive and must match the strings "firstName" and "lastName" in the JSON.
    public string id;
    public string group;
    public string word;
    public string phonetic;
    public string mean;
}

public class Words
{
    //these variables are case sensitive and must match the strings "firstName" and "lastName" in the JSON.
    public List<Word> words;
}

public class GameManager : SingletonMonoBehaviour<GameManager> 
{
    List<string> CORRECT = new List<string> {"すごい！", "まじすごい！", "すばらしい！", "いいね～"};
    List<string> WRONG = new List<string> {"ざんねん～", "がんばって！", "だめだ～"};
    public const string NOT_J_CHARS = "を";
    public const string SMALL_J_CHARS = "ゃゅょっ";
    public const string HARD_J_CHARS = "ばびぶべぼぱぴぷぺぽがぎぐげござじずぜぞだぢづでど";
    public const string J_CHARS = "っあいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわんばびぶべぼぱぴぷぺぽがぎぐげござじずぜぞだぢづでど";
    public TextAsset jsonFile;
 
    public SpriteRenderer sampleCardSprite;
    public Transform startPos;
    public Transform offScrPos;
    public float offset = 0.3f;
    public int numberCols = 5;
    public int numberRows = 5;
    public Text matchedText;
    public Transform cardsDest;
    public Transform showCardsDest;
    public bool isChallengeMode = true;
    // public bool isCorrectMatch = true;
    public Text toggleText;
    public Text showText;
    bool isFalling = false;

    List<Card> selectedCards = new List<Card>();
    List<Card> foundCards = new List<Card>();

    string selectStr = "";
    List<Card> cards = new List<Card>();
    List<Word> words = new List<Word>();
    List<Word> foundedWords = new List<Word>();
    Vector2 cardSize = new Vector2();
    int limitDepth = 0;
    int lastFallIndex = -1;
    string currentString = "";
    int curNode = -1;

    public List<Card> Cards { get => cards; set => cards = value; }
    public bool IsFalling { get => isFalling; set => isFalling = value; }
    public int LastFallIndex { get => lastFallIndex; set => lastFallIndex = value; }
    public List<Card> SelectedCards { get => selectedCards; set => selectedCards = value; }

    void Start()
    {
        AudioManager.Instance.PlayMusic("BGM");
        matchedText.text = "はじめましょう！";
        // GameObject firstCard = CreateCard(startPos.position);
        // SpriteRenderer cardSprite = firstCard.GetComponent<SpriteRenderer>();
        Vector2 bounds = sampleCardSprite.bounds.size;
        cardSize = new Vector2(bounds.x, bounds.y);

        SetupBoard();
        words = ReadWordList();
        limitDepth = 6;
        // limitDepth = MaxLenghtWord();
        // Debug.Log(limitDepth);
        FindWords();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && (selectedCards.Count > 0))
        {
            foreach (Card card in selectedCards)
            {
                selectStr += card.cardText.text;
                // card.SetFlip();
                // if (isCorrectMatch) 
                // {
                //     bool _despaw = selectedCards.IndexOf(card) != 0;
                //     card.SetMatchCorrect(_despaw);
                // }
                // else
                //     card.SetMatchFail();
            }
            Word found = words.Find(w => w.phonetic == selectStr);
            if (found != null) {
                List<int> listIndexes = OnCorrect(found);
                List<Card> newCards = new List<Card>();
                AudioManager.Instance.Shot("Success");
                for (int i = 0; i < selectedCards.Count; i++)
                {
                    int tmpIndex = cards.IndexOf(selectedCards[i]);
                    cards.Remove(selectedCards[i]);
                    Card newCard = CreateCard(selectedCards[i].SelfDest, selectedCards[i].Column, tmpIndex);
                    newCard.Neighbors = selectedCards[i].Neighbors;
                    lastFallIndex = cards.IndexOf(newCard);
                    newCards.Add(newCard);
                }
                StartCoroutine(MoveCards(newCards));
                // newCards.Clear();
                FindWords(listIndexes);
                matchedText.text = CORRECT[Random.Range(0, CORRECT.Count)];
            }
            else
                OnWrong();
            selectStr = "";
            selectedCards.Clear();
        }
    }

    public List<Word> ReadWordList()
    {
        
        Words words = JsonUtility.FromJson<Words>(jsonFile.text);
        return words.words;
        // foreach (Word word in words.words)
        // {
        //     Debug.Log("Found word: " + word.word + " - " + word.phonetic + ": " + word.mean);
        // }
    }
    public void SetupBoard()
    {
        float posX = 0;
        Vector2 nextPos = (Vector2)startPos.position;
        for (int col = 0; col < numberCols; col++) 
		{
            // CreateCard(nextPos);
            float minus = (col % 2 == 0) ? ((cardSize.y + offset) / 2) : 0;
            int added = (minus == 0) ? 1 : 0;
            for (int row = 0; row < numberRows + added; row++) {
                CreateCard(nextPos, col);
                nextPos = new Vector2(startPos.position.x + posX, nextPos.y + cardSize.y + offset);
            }
            posX += cardSize.x + offset;
            nextPos = new Vector2(startPos.position.x + posX, startPos.position.y - minus);
		}
        // Debug.Log(cards);
        lastFallIndex = cards.Count - 1;

        SetupGraph();
        StartCoroutine(MoveCards(cards));
    }

    public void SetupGraph()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            CheckAndAddNeighbor(cards[i], i - 1, true);
            CheckAndAddNeighbor(cards[i], i + 1, true);
            CheckAndAddNeighbor(cards[i], i - 6);
            CheckAndAddNeighbor(cards[i], i + 6);
            CheckAndAddNeighbor(cards[i], i - 5);
            CheckAndAddNeighbor(cards[i], i + 5);
        }
    }
    public void FindWords(List<int> listIndexes=null)
    {
        Debug.Log("Found words");
        if (!(listIndexes != null))
        // {}
            // TODO: need to remove other unwanted to so not for now
            // foundedWords.RemoveAll();
        // else
            foundedWords.Clear(); 
        if (!isChallengeMode)
        {
            if (listIndexes != null)
                foreach (int i in listIndexes)
                {
                    // Debug.Log("Re-search index: " + i);
                    IDDFS(i);
                }
            else
                for (int i = 0; i < cards.Count; i++)
                {
                    IDDFS(i);
                    // Debug.Log(IDDFS(i));
                }
        }
        if (foundedWords.Count == 0)
            ResetGame();
        else {
            string result = "";
            foreach (Word word in foundedWords)
            {
                result += "「" + word.phonetic + "」\n";
            }
            showText.text = result;
        }
        // DebugCards();
    }
    public void ClearBoard()
    {
        foreach (Card card in cards)
        {
            card.ResetState();
            ContentMgr.Instance.Despaw(card.gameObject);
        }
        cards.Clear();
        // foundCards.Clear();
        foundedWords.Clear();
    }

    public void ResetGame() {
        MoveCards(cards, true);
        foreach (Card card in cards)
        {
            card.cardText.text = J_CHARS[Random.Range(0, J_CHARS.Length)].ToString();
        }
        MoveCards(cards);
        // ClearBoard();
        // SetupBoard();
        FindWords();
    }

    // public void ToggleCorrect()
    // {
    //     isCorrectMatch = !isCorrectMatch;
    //     if (isCorrectMatch)
    //         toggleText.text = "Correct";
    //     else
    //         toggleText.text = "Fail";

    // }
    public void AppendCard(Card card)
    {
        selectedCards.Add(card);
    }
    public bool CheckDistance(Card card)
    {
        // TODO: maybe change with neighbors
        if (selectedCards.Count > 0)
        {
            float distWithLastCard = Vector3.Distance(
                selectedCards[selectedCards.Count - 1].gameObject.transform.position, 
                card.gameObject.transform.position);
            if (System.Math.Round(distWithLastCard - MaxDistance(), 1) > 0)
                return false;
        }
        return true;
    }

    int MaxLenghtWord()
    {
        int max = 0;
        Word maxWord = new Word();
        foreach(Word word in words)
        {
            if (word.phonetic.Length > max) 
            {
                max = word.phonetic.Length;
                maxWord = word;
            }
        }
        // Debug.Log("Max word: " + maxWord.word + " - " + maxWord.phonetic + ": " + maxWord.mean);
        return max;
    }
    List<int> OnCorrect(Word found)
    {
        foundedWords.Remove(found);
        List<int> listAroundIndexes = new List<int>();
        foreach (Card card in selectedCards)
        {
            int index = cards.IndexOf(card);
            if (!listAroundIndexes.Contains(index))
                listAroundIndexes.Add(index);
            bool _despaw = selectedCards.IndexOf(card) != 0;
            if (!_despaw) {
                if (foundCards.Count > 0)
                    foundCards[foundCards.Count - 1].SetFlip();
                foundCards.Add(card);
            }
            foreach (int n in card.Neighbors)
            {
                if (!listAroundIndexes.Contains(n))
                    listAroundIndexes.Add(n);
            }
            int i = 0;
            while (i < foundedWords.Count)
            {
                Word word = foundedWords[i];
                if (word.phonetic.Contains(card.cardText.text))
                {
                    int wIndex = int.Parse(word.id);
                    if (!listAroundIndexes.Contains(wIndex)) {
                        listAroundIndexes.Add(int.Parse(word.id));
                    }
                    foundedWords.Remove(word);
                }
                else    
                    i++;
            }
            card.SetMatchCorrect(_despaw, found);
        }
        

        return listAroundIndexes;
    }
    void OnWrong()
    {
        foreach (Card card in selectedCards)
            card.SetMatchFail();
        AudioManager.Instance.Shot("Failed");
        matchedText.text = WRONG[Random.Range(0, WRONG.Count)];
        if (isChallengeMode)
            selectedCards[0].SetFlip();
    }
    float MaxDistance()
    {
        return cardSize.y + offset;
    }

    Card CreateCard(Vector3 pos, int col, int index=-1) 
    {
        Card card = ContentMgr.Instance.GetItem<Card>("Card", offScrPos.position);
        card.cardText.text = J_CHARS[Random.Range(0, J_CHARS.Length)].ToString();
        card.transform.SetParent(startPos);
        card.SelfDest = pos;
        card.Column = col + 1;
        if (index >= 0)
            cards.Insert(index, card);
        else
            cards.Add(card);
        return card;
    }

    void CheckAndAddNeighbor(Card card, int index, bool sameCol=false)
    {
        if (CheckValidIndex(index)) 
        {
            bool colCheck = (sameCol && (cards[index].Column == card.Column)) 
                || (!sameCol && Mathf.Abs(cards[index].Column - card.Column) == 1);
            if (colCheck)
                card.AddNeighbor(index);
        }
    }
    
    bool CheckValidIndex(int index)
    {
        return (0 <= index) && (index < (numberCols * numberRows + numberCols/2));
    }
    
    string IDDFS(int root)
    {
        System.Random r = new System.Random();
        foreach (int depth in Enumerable.Range(1, limitDepth - 1).OrderBy(x => r.Next()))
        {
            currentString = "";
            // Debug.Log("Root: " + root + "; Depth: " + depth);
            int found;
            bool remaining; 
            curNode = root;
            (found, remaining) = DLS(root, depth);
            if (found != -1)
                // Debug.Log("Found word!");   
                return "Found word!";
            if (!remaining)
                return "Not found";
                // Debug.Log("Not found");
        }
        return "Traversal ended";
    }
    
    void DebugCards()
    {
        string strCards = "Cards: ";
        foreach (Card card in cards)
        {
            strCards += "\nIndex: " + cards.IndexOf(card) + "; Char: " + card.cardText.text;
        }
        Debug.Log(strCards);
    }

    (int, bool) DLS(int node, int depth)
    {
        currentString += cards[node].cardText.text;
        // Debug.Log("DLS: " + node + "; Depth: " + depth + "; Cur: " + currentString);
        cards[node].IsVisited = true;
        if (depth == 0) 
        {
            string checkWord = currentString;
            currentString = currentString.Remove(currentString.Length - 1);
            cards[node].IsVisited = false;
            // Debug.Log(node + ": " + checkWord);
            Word findWord = words.Find(w => w.phonetic == checkWord);
            if ((checkWord.Length > 1) && (findWord != null && findWord.phonetic.Length > 0)) 
            {
                if (!(foundedWords.Contains(findWord))) {
                    findWord.id = curNode.ToString();
                    foundedWords.Add(findWord);
                    
                }
                return (node, true);
            }
            else
                return (-1, true);    //(Not found, but may have children)
        }
        else if (depth > 0)
        {
            bool anyRemaining = false;
            foreach (int neighbor in cards[node].Neighbors)
            {
                // Debug.Log("N: " + neighbor + "; Visited: " + cards[neighbor].IsVisited);
                if (!cards[neighbor].IsVisited) 
                {
                    // cards[neighbor].Parent = node;
                    int found;
                    bool remaining; 
                    (found, remaining) = DLS(neighbor, depth-1);
                    if (found != -1)
                        return (found, true);   
                    if (remaining)
                        anyRemaining = true;   
                } //(At least one node found at depth, let IDDFS deepen)
            }
            cards[node].IsVisited = false;

            currentString = currentString.Remove(currentString.Length - 1);
            return (-1, anyRemaining);
        }
            
        return (-1, false);    //(Not found, but may have children)

    }
    IEnumerator MoveCards(List<Card> _cards, bool isHide=false) 
    {
        isFalling = true;
        foreach (Card card in _cards) 
        {
            card.SetUpperLayer();
            if (!isHide)
                card.MoveToPos(card.SelfDest);
            else
                card.MoveToPos(offScrPos.position);

            yield return new WaitForSeconds(0.05f);
            // yield return null;
        }
        if (_cards != cards)
            _cards.Clear();
        // isFalling = false;
    }
}

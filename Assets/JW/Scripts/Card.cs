using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

    

public class Card : MonoBehaviour
{
    
    public SpriteRenderer cardSprite;
    public Canvas cardCanvas;
    public Text cardText;
    public Color activeColor = new Color();
    public Sprite backSprite;
    public Sprite frontSprite;
    public float flyTime = 1f;
    public float maxScale = 0.4f;

    [Header("Shake")]
    [Range(0f, 2f)]
    public float time = 0.2f;
    [Range(0f, 2f)]
    public float distance = 0.1f;
    [Range(0f, 0.1f)]
    public float delayBetween = 0f;
    [Header("Show card")]
    public float showSpeed = 1f;
    public float showScale = 0.8f;
    float _showScale = 0.0f;

    bool _isChoose = false;
    bool _isFlipDown = false;
    bool _isDespaw = true;
    float _orgScale;
    float _stepScale = 0.01f;
    Vector3 _startPos;
    Vector3 _selfDest;
    bool _isVisited = false;
    // int parent = -1;
    int column = -1;
    Word foundWord;
    List<int> neighbors = new List<int>();

    public Vector3 SelfDest { get => _selfDest; set => _selfDest = value; }
    public List<int> Neighbors { get => neighbors; set => neighbors = value; }
    public bool IsVisited { get => _isVisited; set => _isVisited = value; }
    // public int Parent { get => parent; set => parent = value; }
    public int Column { get => column; set => column = value; }

    // Start is called before the first frame update
    void Start()
    {
        _orgScale = transform.localScale.x;
        _startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
    }
    void OnMouseOver()
    {
        if (Input.GetMouseButton(0) && !GameManager.Instance.IsFalling)
            SetToActive();
    }
    public void AddNeighbor(int index)
    {
        neighbors.Add(index);
    }

    public void SetToActive()
    {
        if (!_isFlipDown && !_isChoose && GameManager.Instance.CheckDistance(this))
        {
            // Debug.Log("You activated " + cardText.text);
            cardSprite.color = activeColor;
            _isChoose = true;
            GameManager.Instance.AppendCard(this);
        }
    }

    public void SetToNormal()
    {
        cardSprite.color = new Color(255, 255, 255, 255);
        _isChoose = false;
    }

    public void SetFlip()
    {
        _isFlipDown = true;
        cardSprite.color = new Color(255, 255, 255, 255);
        AudioManager.Instance.Shot("Flip");
        StartCoroutine("FlipCard");
    }

    public void SetMatchCorrect(bool _despaw, Word word)
    {
        _isDespaw = _despaw;
        foundWord = word;
        cardSprite.color = new Color(255, 255, 255, 255);
        _isFlipDown = true;
        transform.position = transform.position * (1 + maxScale);
        SetUpperLayer(5, 6);
        StartCoroutine(ScaleUp(GameManager.Instance.showCardsDest.position, maxScale));
    }
    public void SetMatchFail()
    {
        cardSprite.color = new Color(255, 255, 255, 255);
        StartCoroutine("Shake");
    }

    public void SetUpperLayer(int cardLayer=3, int textLayer=4)
    {
        cardSprite.sortingOrder = cardLayer;
        cardCanvas.sortingOrder = textLayer;
    }

    public void SetLowerLayer()
    {
        cardSprite.sortingOrder = 1;
        cardCanvas.sortingOrder = 2;
    }

    public void MoveToPos(Vector3 pos)
    {
        if (pos == Vector3.zero)
            pos = _selfDest;
        _isChoose = true;
        StartCoroutine(MoveToDest(pos));
    }

    public void ResetState()
    {
        _isChoose = false;
        _isFlipDown = false;
        SetLowerLayer();
        cardSprite.sprite = frontSprite;
        cardText.gameObject.SetActive(true);
        transform.localScale = new Vector3(_orgScale, _orgScale, 1);
        
        cardText.text = GameManager.J_CHARS[Random.Range(0, GameManager.J_CHARS.Length)].ToString();
        // transform.Rotate(0, 0, 0);
    }

    IEnumerator FlipCard() 
    {
        for (int angle = 0; angle <= 180; angle += 10) 
        {
            transform.eulerAngles = new Vector3(0, angle, 0);
            // gameObject.transform.Rotate(0, angle, 0);
            if (angle == 90) 
            {
                cardSprite.sprite = backSprite;
                cardText.gameObject.SetActive(false);
            }
            // if (angle == 180)
            // {
            //     // StopCoroutine("FlipCard");
            //     yield break;

            // }
            yield return null;
        }
    }

    IEnumerator ScaleUp(Vector3 dest, float scaleTo) 
    {
        for (float scale = _orgScale; scale <= maxScale; scale += _stepScale) 
        {
            transform.localScale = new Vector3(scale, scale, 1);
            // gameObject.transform.Rotate(0, angle, 0);
            if (scale >= maxScale - _stepScale)
            {
                yield return new WaitForSeconds(0.25f);
                // SetFlip();
                // StartCoroutine("FlipCard");
                // StartCoroutine("ScaleDown");
                _showScale = scale;
                StartCoroutine(MoveToDest(dest, 1.5f));
                // StopCoroutine("ScaleUp");
                yield break;

            }
            yield return null;
        }
    }

    IEnumerator ScaleDown() 
    {
        for (float scale = maxScale; scale >= _orgScale; scale -= _stepScale) 
        {
            transform.localScale = new Vector3(scale, scale, 1);
            // gameObject.transform.Rotate(0, angle, 0);
            yield return null;
        }
        // StopCoroutine("ScaleDown");
    }

    IEnumerator MoveToDest(Vector3 dest, float defaultSpeed = 1) 
    {
        float _elapsedTime = 0.0f;
        float _textAlpha = 1.0f;
        int _count = 0;
        

        while (_elapsedTime < flyTime)
        {
            transform.position = Vector3.Lerp(transform.position, dest, (_elapsedTime * 0.5f / flyTime));
            _elapsedTime += Time.deltaTime * defaultSpeed;
            if (dest != _selfDest) 
            {
                if (dest != GameManager.Instance.cardsDest.position) 
                {
                    _showScale += _stepScale * defaultSpeed;
                    _textAlpha -= 0.013f * defaultSpeed;
                    cardText.CrossFadeAlpha(_textAlpha, 0f, true);
                }
                else
                {
                    _showScale -= _stepScale * defaultSpeed * 0.7f;
                    if (_showScale < 0.5f)
                        _showScale = 0.5f;
                }
                transform.localScale = new Vector3(_showScale, _showScale, 1);
            }
                
            _count += 1;
            
            if (_count == 30)
                AudioManager.Instance.Shot("Place");
            yield return null;
        }  
        // Make sure we got there
        transform.position = dest;
        if (dest == _selfDest) 
        {
            SetLowerLayer();

            if (GameManager.Instance.Cards.IndexOf(this) == GameManager.Instance.LastFallIndex)
                GameManager.Instance.IsFalling = false;
            
            _isChoose = false;
            _startPos = _selfDest;
        }
        else
        {

            if (_isDespaw)
            {
                cardText.gameObject.SetActive(false);
                ResetState();
                ContentMgr.Instance.Despaw(gameObject);
            }
            else {
                // StartCoroutine(ScaleUp(GameManager.Instance.cardsDest.position, showScale));
                SetUpperLayer();
                cardText.CrossFadeAlpha(1f, 0f, true);
                cardText.text = "<b>" + foundWord.phonetic 
                    + "</b>\n_______\n\n" + foundWord.word + "\n\n" + foundWord.mean;
                cardText.fontSize = 1;
                cardText.alignment = TextAnchor.UpperCenter;
                yield return new WaitForSeconds(0.5f);
                if (dest != GameManager.Instance.cardsDest.position)
                    StartCoroutine(MoveToDest(GameManager.Instance.cardsDest.position, 3f));
                // else
                //     transform.localScale = new Vector3(0.39f, 0.39f, 1f);

            }
        }
        yield break;
        // yield return null;
        // transform.position = Vector3.Lerp(transform.position, dest, Time.deltaTime);
    }

    IEnumerator Shake()
    {
        float _timer = 0f;
        Vector3 _randomPos = new Vector3();
    
        while (_timer < time)
        {
            _timer += Time.deltaTime;
    
            _randomPos = _startPos + (Random.insideUnitSphere * distance);
    
            transform.position = _randomPos;
    
            if (delayBetween > 0f)
            {
                yield return new WaitForSeconds(delayBetween);
            }
            else
            {
                yield return null;
            }
        }
        _isChoose = false;
        transform.position = _startPos;
    }
}

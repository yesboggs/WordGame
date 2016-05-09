using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public enum GameMode
{
    preGame,
    loading,
    makeLevel,
    levelPrep,
    inLevel
}
public class WordGame : MonoBehaviour
{
    static public WordGame S;

    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(.8f, .8f, .8f);
    public Color bigColorSelected = Color.white;
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
    public List<float> scoreFontSizes = new List <float> {24, 36, 36, 1};
    public Vector3 scoreMidPoint = new Vector3(1, 1, 0);
    public float scoreComboDelay = .5f;
    public Color[] wyrdPalette;

    public bool _____________;

    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWord;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    void awake()
    {
        S = this;
    }
    // Use this for initialization
    void Start()
    {
        mode = GameMode.loading;
        WordList.S.Init();
    }

    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;
        currLevel = MakeWordLevel();
    }
    // Update is called once per frame
    void Update()
    {
        Letter lett;
        char c;
        switch (mode)
        {
            case GameMode.inLevel:
                foreach(char cIt in Input.inputString)
                {
                    c = System.Char.ToUpperInvariant(cIt);
                    if (upperCase.Contains(c))
                    {
                        lett = FindNextLetterByChar(c);
                        if(lett!= null)
                        {
                            testWord += c.ToString();
                            bigLettersActive.Add(lett);
                            bigLetters.Remove(lett);
                            lett.color = bigColorSelected;
                            ArrangeBigLetters();
                        }
                    }
                    if(c == '\b')
                    {
                        if (bigLettersActive.Count == 0) return;
                        if(testWord.Length > 1)
                        {
                            testWord = testWord.Substring(0, testWord.Length - 1);

                        }
                        else
                        {
                            testWord = "";
                        }
                        lett = bigLettersActive[bigLettersActive.Count - 1];
                        bigLettersActive.Remove(lett);
                        lett.color = bigColorDim;
                        ArrangeBigLetters();
                    }
                    if(c =='\n' || c == '\r')
                    {
                        CheckWord();
                    }
                    if(c == '\n')
                    {
                        StartCoroutine(CheckWord());
                    }
                    if(c == ' '){
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }break;
        }
        
    }
    
    Letter FindNextLetterByChar(char c)
    {
        foreach (Letter l in bigLetters){
            if(l.c == c){
                return (l);
            }
        }
        return (null);
    }
    public IEnumerator CheckWord()
    {
        string subWord;
        bool foundTestWord = false;
        List<int> containedWords = new List<int>();

        for (int i=0; i<currLevel.subWords.Count; i++)
        {
            if (wyrds[i].found)
            {
                continue;
            }
            subWord = currLevel.subWords[i];
            if(string.Equals(testWord, subWord))
            {
                HighlightWyrd(i);
                Score(wyrds[i], 1);
                foundTestWord = true;
            }
            else if (testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }
        if (foundTestWord)
        {
            int numContained = containedWords.Count;
            int ndx;
            for(int i=0; i<containedWords.Count; i++)
            {
                yield return (new WaitForSeconds(scoreComboDelay));

                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                Score(wyrds[containedWords[ndx]], i + 2);
            }
        }
        ClearBigLettersActive();
    }

    void HighlightWyrd(int ndx)
    {
        wyrds[ndx].found = true;
        wyrds[ndx].color = (wyrds[ndx].color + Color.white) / 2f;
        wyrds[ndx].visible = true;
    }

    void ClearBigLettersActive()
    {
        testWord = "";
        foreach (Letter l in bigLettersActive)
        {
            bigLetters.Add(l);
            l.color = bigColorDim;
        }
        bigLettersActive.Clear();
        ArrangeBigLetters();
    }
    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if (levelNum == -1)
        {
            level.longWordIndex = Random.Range(0, WordList.S.longWordCount);
        }
        else
        {

        }
        level.levelNum = levelNum;
        level.word = WordList.S.GetLongWord(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);

        StartCoroutine(FindSubWordsCoroutine(level));
        return (level);
    }
    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;

        List<string> words = WordList.S.GetWords();

        for (int i = 0; i < WordList.S.wordCount; i++)
        {
            str = words[i];
            if (WordLevel.CheckWordInLevel(str, level))
            {
                level.subWords.Add(str);
            }
            if (i % WordList.S.numToParseBeforeYield == 0)
            {
                yield return null;
            }
        }
        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();
        SubWordSearchComplete();
    }
    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> e)
    {
        var sorted = from s in e
                     orderby s.Length ascending
                     select s;
        return sorted;
    }
    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout();
    }
    void Layout()
    {
        wyrds = new List<Wyrd>();

        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];

            columnWidth = Mathf.Max(columnWidth, word.Length);


            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];
                go = Instantiate(prefabLetter) as GameObject;
                lett = go.GetComponent<Letter>();

                lett.c = c;
                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);
                pos.y -= (i % numRows) * letterSize;
                lett.position = pos + Vector3.up * (20 + i % numRows);
                lett.pos = pos;
                lett.timeStart = Time.time + i * .05f;
                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }
            if (showAllWyrds) wyrd.visible = true;
            wyrd.color = wyrdPalette[word.Length - WordList.S.wordLengthMin];
            wyrds.Add(wyrd);

            if (i % numRows == numRows - 1)
            {
                left += (columnWidth + .5f) * letterSize;
            }
        }
        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();
        for (int i = 0; i < currLevel.word.Length; i++)
        {
            c = currLevel.word[i];
            go = Instantiate(prefabLetter) as GameObject;
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            pos = new Vector3(0, -100, 0);
            lett.pos = pos;
            lett.timeStart = Time.time + currLevel.subWords.Count * .05f;
            lett.easingCuve = Easing.Sin + "-0.18";
            col = bigColorDim;
            lett.color = col;
            lett.visible = true;
            lett.big = true;
            bigLetters.Add(lett);
        }
        bigLetters = ShuffleLetters(bigLetters);
        ArrangeBigLetters();
        mode = GameMode.inLevel;
    }
    List<Letter> ShuffleLetters(List<Letter> letts)
    {
        List<Letter> newL = new List<Letter>();
        int ndx;
        while (letts.Count > 0)
        {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return (newL);
    }

    void ArrangeBigLetters()
    {
        float halfwidth = ((float)bigLetters.Count) / 2f - .5f;
        Vector3 pos;
        for(int i=0; i<bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfwidth) * bigLetterSize;
            bigLetters[i].pos = pos;

        }
        halfwidth = ((float)bigLettersActive.Count) / 2f - .5f;
        for(int i=0; i<bigLettersActive.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfwidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLettersActive[i].pos = pos;
        }
    }
    void Score(Wyrd wyrd, int combo)
    {
        Vector3 pt = wyrd.letters[0].transform.position;
        List<Vector3> pts = new List<Vector3>();

        pt = Camera.main.WorldToViewportPoint(pt);
        pt.z = 0;
        pts.Add(pt);
        pts.Add(scoreMidPoint);
        pts.Add(Scoreboard.S.transform.position);
        int value = wyrd.letters.Count * combo;
        FloatingScore fs = Scoreboard.S.CreateFloatingScore(value, pts);

        fs.timeDuration = 2f;
        fs.fontSizes = scoreFontSizes;
        fs.easingCurve = Easing.InOut + Easing.InOut;
        string txt = wyrd.letters.Count.ToString();
        if (combo > 1){
            txt += " x " + combo;

        }
        fs.GetComponent<GUIText>().text = txt;
    }
}
    


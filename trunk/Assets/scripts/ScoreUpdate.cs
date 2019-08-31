using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using conilines.engine;
using System;


class ScoreUpdate : MonoBehaviour
{
    private TMPro.TextMeshPro mytext;
    private int score;

    public int Score { get => score; set {
            score = value;
            PrintScore();
        } }
    private GameField field;
    private void PrintScore()
    {
        mytext.text = "Score: " + Score.ToString();
    }

    // Start is called before the first frame update
    private void Awake()
    {
        field = null;
    }

    private void SetField(object sender, EventArgs e)
    {
        if (!(field is null))
        {
            field.TokensKilled -= CalculateScore;
        }
        try
        {
            field = TheGame.Me.Field;
            field.TokensKilled += CalculateScore;
            ResetScore();
        }catch(ArgumentOutOfRangeException)
        {
            score = 0;
        }
    }

    void Start()
    {
        mytext = GetComponent<TMPro.TextMeshPro>();
        TheGame.Me.FieldSet += SetField;
        SetField(this, new EventArgs());        
    }

    private void ResetScore()
    {
        Score = 0;
    }

    private void CalculateScore(object sender, TokenEventArgs e)
    {
        int asc = 0;
        int cnt = 0;
        e.Tokens.ForEach(delegate (GameTokenData t)
        {
            asc += t.value;
            cnt++;
        });
        asc *= (cnt / 5) + 1;
        Score += asc;        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

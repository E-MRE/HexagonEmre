using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : BaseMonoBehaviour
{
    //Variables
    public static ScoreManager instance;
    public int score { get; set; }
    public int moveCount { get; set; }
    public Text scoreText, moveText;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        score = 0;
        moveCount = 0;

        scoreText.text = score.ToString();
        moveText.text = moveCount.ToString();
    }

    //It adds score using exploded hexagons. Each hexagon multiply with SCORE_VALUE
    public void AddScore(int hexagonCount)
    {
        CheckBomb(hexagonCount);
        score += hexagonCount * SCORE_VALUE;
        scoreText.text = score.ToString(); 
    }

    //Checks bomb is available
    private void CheckBomb(int newScore)
    {
        if (score < BOMB_VALUE && (score + (newScore * SCORE_VALUE)) > BOMB_VALUE)
        {
            BOMB_VALUE += 1000;
            BOMB_READY = true;
        }
    }

    //It adds user move after the explosion hexagons
    public void AddMove()
    {
        moveCount += 1;
        moveText.text = moveCount.ToString();

        SetBombTimer(GridManager.instance.GetBombList());
    }

    //This function decreases bomb timer
    private void SetBombTimer(List<Hexagon> bombList)
    {
        if(bombList!=null && bombList.Count > 0)
        {
            foreach (var bomb in bombList)
            {
                bomb.SetTimer(bomb.GetTimer() - 1);
            }
        }
    }

    //For restart game
    public void RetryGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class UISettings : MonoBehaviour
    {
        [SerializeField] private Text scoreText;

        private void Start()
        {
            if (SceneManager.GetActiveScene().buildIndex == 1)
                ChaneScore();
        }

        public void ToGame() => SceneManager.LoadScene("Game");

        public void ApplQuit() => Application.Quit();

        public void ChaneScore() => scoreText.text = $"Wins: {PlayerPrefs.GetInt("CurrentScore")}";
    }
}
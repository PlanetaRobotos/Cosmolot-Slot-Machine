using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UISettings uiSettings;

    [SerializeField] private float rowYRepeatPoint;
    [SerializeField] private float rowYStartPoint;
    [SerializeField] private Transform[] rows;
    [SerializeField] private float speed;
    [SerializeField] private float[] ySlotPositions;


    [SerializeField] private int nextFalseMatch = 2;
    [SerializeField] private float[] speedsForMatch;

    [SerializeField] private GameObject winPrefab;

    [Header("Audio")] [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip spinClip;
    [SerializeField] private AudioClip coinMinusClip;

    [SerializeField] private Image clickImage;
    private Color _defColor;

    private float[] _currentYPositions;
    private int _check;
    private int _buildIndex;
    private int _currentSpin;
    private int _spinOffset;
    private bool _canClick;

    private AudioSource _audioSource;
    [SerializeField] private Vector3 maxScale = Vector3.one * 3;

    // private int del;


    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _currentYPositions = new float[3];
        _canClick = true;
        _defColor = clickImage.color;

        _currentSpin = 1;
        _buildIndex = SceneManager.GetActiveScene().buildIndex;
        if (_buildIndex == 0)
            StartCoroutine(EndlessRotation());
    }

    /// <summary>
    /// Work only in intro scene
    /// </summary>
    /// <returns></returns>
    private IEnumerator EndlessRotation()
    {
        while (gameObject.activeInHierarchy)
        {
            foreach (Transform row in rows)
                StartCoroutine(RotateRow(row));

            _audioSource.PlayOneShot(coinMinusClip);
            _audioSource.PlayOneShot(spinClip);

            yield return new WaitUntil(() => _check >= rows.Length);
            yield return new WaitForSeconds(1f);
            _check = 0;
        }
    }

    private void Update()
    {
        clickImage.color = _canClick ? _defColor : Color.clear;
    }

    /// <summary>
    /// When button clicked
    /// </summary>
    public void Rotate()
    {
        if (!_canClick) return;
        _audioSource.PlayOneShot(coinMinusClip);
        _audioSource.PlayOneShot(spinClip);

        if (_currentSpin % nextFalseMatch == 0)
            _spinOffset = Random.Range(0, 7);

        foreach (Transform row in rows)
            StartCoroutine(RotateRow(row));

        StartCoroutine(AwaitReadyToCheckIe());
        _canClick = false;
    }

    /// <summary>
    /// Work after rows stops - check match and instantiate win coins
    /// </summary>
    /// <returns></returns>
    private IEnumerator AwaitReadyToCheckIe()
    {
        yield return new WaitUntil(() => _check >= rows.Length);

        _check = 0;
        _currentSpin++;
        Debug.Log(_currentSpin);
        if (CheckMatches())
        {
            int newScore = PlayerPrefs.GetInt("CurrentScore") + 1;
            PlayerPrefs.SetInt("CurrentScore", newScore);
            uiSettings.ChaneScore();
            StartCoroutine(WinIe());

            _audioSource.PlayOneShot(winClip);
        }
        else
        {
            _audioSource.PlayOneShot(loseClip);
        }

        _canClick = true;
    }

    private IEnumerator WinIe()
    {
        GameObject win = Instantiate(winPrefab, Vector3.zero, Quaternion.identity);
        while (Vector3.Distance(win.transform.localScale, maxScale) > 0.1f)
        {
            var scale = win.transform.localScale;
            win.transform.localScale = Vector3.Lerp(scale, maxScale, Time.deltaTime);
            yield return null;
        }
        Destroy(win, 0.3f);
    }

    /// <summary>
    /// Main logic for rows (rotation, set slots type)
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    private IEnumerator RotateRow(Transform row)
    {
        float currSpeed = speed;
        if (_currentSpin % nextFalseMatch == 0 && _buildIndex == 1)
        {
            row.position = new Vector2(row.position.x, ySlotPositions[2]);
            currSpeed = speedsForMatch[Random.Range(0, speedsForMatch.Length)];
            // currSpeed = speedsForMatch[del];
            // del++;
            Debug.Log($"c: {currSpeed}");
        }
        else
            currSpeed += Random.Range(-3f, 3f);

        while (currSpeed >= 0.2f)
        {
            var rowPosition = row.position;
            rowPosition += Vector3.down * (currSpeed * Time.deltaTime);
            row.position = rowPosition;
            currSpeed -= Time.deltaTime;

            Vector3 position = rowPosition;
            if (position.y <= rowYRepeatPoint)
                row.position = new Vector2(position.x, rowYStartPoint);

            yield return null;
        }

        float mostNextTo = 100;
        int j = 0;
        for (int i = 0; i < ySlotPositions.Length; i++)
        {
            float dist = Mathf.Abs(row.position.y - ySlotPositions[i]);
            if (dist < mostNextTo)
            {
                mostNextTo = dist;
                j = i;
            }
        }

        float ySlotPosition = ySlotPositions[j];

        while (Mathf.Abs(row.position.y - ySlotPosition) > 0.05f)
        {
            var position = row.position;
            position = Vector3.Lerp(position,
                new Vector2(position.x, ySlotPosition), Time.deltaTime * 1f);
            row.position = position;
            yield return null;
        }

        var rPos = row.position;
        rPos = new Vector2(rPos.x, ySlotPosition);
        row.position = rPos;

        // _currentView[_readyToCheck] = _slots[ySlotPosition];
        _currentYPositions[_check] = rPos.y;
        _check++;
    }

    private bool CheckMatches()
    {
        // return _currentView.Count(t => _currentView[0] == t) == _currentView.Length;
        int counter = 0;
        for (int i = 0; i < _currentYPositions.Length; i++)
            if (_currentYPositions[0] == _currentYPositions[i])
                counter++;

        return counter >= _currentYPositions.Length;
    }
}
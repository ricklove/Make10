using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.Advertisements;

public class Game : MonoBehaviour
{
    public AudioClip winSound;

    public GameObject bubblePrefab;
    public GameObject bubbleAreaPrefab;

    private List<BubbleAreaPhysics> bubbleAreas = new List<BubbleAreaPhysics>();
    private int total;

    private GameState gameState;
    private GameState lastGameState;

    private BackgroundChanger backgroundChanger;

    void Start()
    {
        gameState = GameState.Title;
        backgroundChanger = transform.root.GetComponentInChildren<BackgroundChanger>();

        Advertisement.Initialize("22043");
    }


    void Update()
    {
        var hasChanged = lastGameState != gameState;
        lastGameState = gameState;

        switch (gameState)
        {
            case GameState.Title:
                UpdateTitle();
                break;
            case GameState.Create:
                UpdateCreate();
                break;
            case GameState.Active:
                UpdateActive();
                break;
            case GameState.Win:
                UpdateWin(hasChanged);
                break;
            case GameState.Ads:
                UpdateAds(hasChanged);
                break;
            default:
                break;
        }
    }

    private void UpdateTitle()
    {
        // If click, goto game

        // TODO: Implement Title
        gameState = GameState.Create;
    }

    private void UpdateCreate()
    {
        // Change background
        backgroundChanger.ChangeBackground();

        var root = gameObject;

        // Create game two areas
        var posA = new Vector2(Random.RandomRange(-20, -10), Random.RandomRange(-10, 10));
        var posB = new Vector2(Random.RandomRange(10, 20), Random.RandomRange(-10, 10));

        var areaA = Instantiate(bubbleAreaPrefab, posA, Quaternion.identity) as GameObject;
        var areaB = Instantiate(bubbleAreaPrefab, posB, Quaternion.identity) as GameObject;

        areaA.transform.parent = root.transform;
        areaB.transform.parent = root.transform;

        bubbleAreas.Clear();
        bubbleAreas.Add(areaA.GetComponent<BubbleAreaPhysics>());
        bubbleAreas.Add(areaB.GetComponent<BubbleAreaPhysics>());


        // Create random number of bubbles in each area
        var valA = Random.Range(2, 10);
        total = Random.Range(11, 20);
        var valB = total - valA;

        while (valA >= 10 || valB >= 10 || total <= 10)
        {
            valA = Random.Range(2, 10);
            total = Random.Range(11, 20);
            valB = total - valA;
        }

        CreateBubblesInArea(root, posA, valA);
        CreateBubblesInArea(root, posB, valB);

        // Change to active
        gameState = GameState.Active;
    }

    private void RemoveBubbles()
    {
        // Remove all game objects
        var root = gameObject;

        var children = new List<GameObject>();
        foreach (Transform child in transform) { children.Add(child.gameObject); }
        children.ForEach(child =>
        {
            Destroy(child);
        });
    }

    private void CreateBubblesInArea(GameObject root, Vector2 pos, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var bPos = new Vector2(Random.RandomRange(-3, 3) + pos.x, Random.RandomRange(-3, 3) + pos.y);
            var bubble = Instantiate(bubblePrefab, bPos, Quaternion.identity) as GameObject;
            bubble.transform.parent = root.transform;
        }
    }

    private float timeAtCorrect;

    private void UpdateActive()
    {
        // Check for win
        var isCorrect = false;

        if (BubblePhysics.LiveBubbles.All(b => b.IsAttachedToBubbleArea))
        {
            if (bubbleAreas.Any(a => a.attachedBubbles.Count == 10))
            {
                isCorrect = true;

                if (timeAtCorrect == 0)
                {
                    timeAtCorrect = Time.time;
                }
                else if (Time.time - timeAtCorrect > 1f)
                {
                    bubbleAreas.ForEach(a => a.LockBubbles());
                    gameState = GameState.Win;
                }
            }
        }

        if (!isCorrect) { timeAtCorrect = 0; }
    }

    private float timeAtWin;

    private int winCount = 0;

    private void UpdateWin(bool hasChanged)
    {
        if (hasChanged)
        {
            timeAtWin = Time.time;
            winCount++;

            // Show win
            var winArea = bubbleAreas.First(a => a.attachedBubbles.Count == 10);
            var loseArea = bubbleAreas.First(a => a != winArea);

            // Add win sound
            if (winSound != null)
            {
                audio.PlayOneShot(winSound);
            }

            // Remove inner bubbles
            foreach (var b in winArea.attachedBubbles)
            {
                b.gameObject.SetActive(false);
            }

            loseArea.gameObject.GetComponent<BubbleText>().text = "" + (total - 10);

            // Intersect with small bubbles
            //winArea.gameObject.layer = winArea.attachedBubbles.First().gameObject.layer;

            // Disolve other areas
            //bubbleAreas.Where(a => a != winArea).ToList().ForEach(a => a.gameObject.SetActive(false));
        }

        var timePassed = Time.time - timeAtWin;

        if (timePassed > 3)
        {
            gameState = GameState.Ads;
        }
    }

    private void UpdateAds(bool hasChanged)
    {
        if (hasChanged)
        {
            var countBetweenAds = 1;//10;

            RemoveBubbles();

            // Show Unity Ads once
            if (winCount % (countBetweenAds * 2) == countBetweenAds)
            {
                AdRequestController.Instance.RequestGameVideo("Completed " + winCount + "th Puzzle", () =>
                {
                    gameState = GameState.Create;
                });
            }
            // Show Kiip Rewards whenever Kiip determines
            else if (winCount % (countBetweenAds * 2) == 0)
            {
                AdRequestController.Instance.RequestReward("Completed " + winCount + "th Puzzle", () =>
                {
                    gameState = GameState.Create;
                });
            }
            else
            {
                gameState = GameState.Create;
            }
        }
    }
}

public enum GameState
{

    Title,
    Create,
    Active,
    Win,
    Ads

}

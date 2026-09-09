using Cysharp.Threading.Tasks;
using ProDomino.Authentication;
using ProDomino.GameSystem;
using ProDomino.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Timba.Patterns;
using TMPro;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

namespace ProDomino.Leaderboard
{
    public class PlayerBestRankController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image leaderboardIcon;
        [SerializeField] private TMP_Text rankScoreLabel;
        
        private GameManager gameManager;
        private AuthManager authManager;
        private LeaderboardManager leaderboardManager;
        private DictionaryService dictionaryService;

        private Sprite defaultRankIcon;
        private string defaultRankScoreLabel;

        public KeyValuePair<string, LeaderboardEntry> PlayerBestRankEntry { get; private set; }

        private void Awake()
        {
            // Get the required services from the service locator
            gameManager = ServiceLocator.Instance.GetService<GameManager>();
            authManager = ServiceLocator.Instance.GetService<AuthManager>();
            leaderboardManager = ServiceLocator.Instance.GetService<LeaderboardManager>();
            dictionaryService = ServiceLocator.Instance.GetService<DictionaryService>();

            // Check if the canvas group is assigned and set it to inactive by default
            if (leaderboardIcon)
                defaultRankIcon = leaderboardIcon.sprite;

            // Check if the rank score label is assigned and get its default text
            if (rankScoreLabel)
                defaultRankScoreLabel = rankScoreLabel.text;

            if (!gameManager || !authManager || !dictionaryService || !leaderboardManager )
            {
                Debug.LogError("Required services are not initialized. Cannot proceed with PlayerBestRankController initialization.");
                return;
            }

            leaderboardManager.AddListener_OnPlayerLeaderboardDataUpdated(Configure);

            // By default, hide the canvas group
            if (canvasGroup)
                canvasGroup.SetActive(false);

            // Add listener for sign in event
            gameManager?.HandleOnSignIn(OnSignIn);

            // Add listener for sign out event
            gameManager?.HandleOnSignOut(OnSignOut);

            // Wait until the GameManager is initialized
            if (gameManager.IsAlreadyInitialized)
                OnSignIn();
            else
                OnSignOut();
        }

        private void OnDestroy()
        {
            if (leaderboardManager != null)
                leaderboardManager.RemoveListener_OnPlayerLeaderboardDataUpdated(Configure);
            else
                Debug.LogWarning("LeaderboardManager is null. Cannot remove listener.");
        }

        public void Configure(Dictionary<string, LeaderboardEntry> playerLeaderboards)
        {
            // Check if the player leaderboards data is null or empty
            if (playerLeaderboards is null or { Count: 0 })
            {
                Debug.LogWarning("Player leaderboards data is null or empty. Setting default data...");

                // Reset the leaderboard icon and rank score label to their default values
                SetDefaultValues();
                return;
            }

            // Order the player leaderboards by score and get the top entry
            var topScoreKvp = playerLeaderboards
                .OrderByDescending(entry => entry.Value.Score)
                .FirstOrDefault();

            // Check if the top score entry is null or if its value is null
            if (topScoreKvp.Value is null)
            {
                Debug.LogWarning("No valid leaderboard entry found.");
                return;
            }

            // Set the player's best rank entry
            PlayerBestRankEntry = topScoreKvp;

            // Set the leaderboard icon using the rank sprite
            var rankIcon = dictionaryService.GetSprite(Consts.CollectionKeys.Ranks, PlayerBestRankEntry.Value.Tier);
            if (leaderboardIcon && rankIcon != null)
                leaderboardIcon.sprite = rankIcon;
            else
                Debug.LogWarning("Rank icon is null or Image component is not assigned.");

            // Set the rank score label
            if (rankScoreLabel && PlayerBestRankEntry.Value is LeaderboardEntry leaderboardEntry)
            {
                var tier = Regex.Replace(leaderboardEntry.Tier, "(?<!^)([A-Z])", " $1");
                rankScoreLabel.text = $"{tier} {leaderboardEntry.Score}";
            }
            else
                Debug.LogWarning("Rank score label is not assigned.");
        }

        private void SetDefaultValues()
        {
            // Reset the leaderboard icon to the default sprite
            if (leaderboardIcon && defaultRankIcon != null)
                leaderboardIcon.sprite = defaultRankIcon;
            else
                Debug.LogWarning("Leaderboard icon is not assigned or default rank icon is null.");

            // Reset the rank score label to the default text
            if (rankScoreLabel && !string.IsNullOrEmpty(defaultRankScoreLabel))
                rankScoreLabel.text = defaultRankScoreLabel;
            else
                Debug.LogWarning("Rank score label is not assigned or default rank score label is null.");
        }

        private void OnSignIn()
        {
            if (canvasGroup && gameManager is not null and { IsAlreadyInitialized: true, IsAuthenticated: true })
                canvasGroup.SetActive(true);
        }

        private void OnSignOut()
        {
            // Block all navigation buttons that require authentication
            if (canvasGroup)
            {
                // Reset the leaderboard icon and rank score label to their default values
                SetDefaultValues();
                canvasGroup.SetActive(false);
            }
        }
    }
}

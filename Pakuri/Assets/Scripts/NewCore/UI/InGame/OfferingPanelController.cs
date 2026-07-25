using System;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Run.Services;
using Pakuri.NewCore.Units.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* Offering panel의 후보 표시와 선택 command를 소유한다. */
namespace Pakuri.NewCore.UI.InGame
{
    public sealed class OfferingPanelController : MonoBehaviour
    {
        [SerializeField] private GameBootstrap combatManager;

        private readonly Button[] offeringButtons = new Button[3];
        private GameObject offeringPanel;
        private OfferingOffer activeOffer;
        private Action completed;

        /* bootstrap과 완료 callback을 연결하고 authored Offering hierarchy를 찾는다. */
        public void Initialize(
            GameBootstrap runtime,
            Action onCompleted)
        {
            combatManager ??= runtime;
            completed = onCompleted
                ?? throw new ArgumentNullException(nameof(onCompleted));
            offeringPanel = FindObject("OfferingPanel");
            for (int index = 0; index < offeringButtons.Length; index++)
            {
                offeringButtons[index] = Find<Button>(
                    $"OfferingPanel/Choice{index + 1}");
            }
            Hide();
        }

        /* 선택 Monster와 포로에서 Offering 후보를 만들고 세 button에 표시한다. */
        public bool Open(MonsterModel monster, Prisoner prisoner)
        {
            activeOffer = combatManager.Offerings.GenerateCandidates(
                monster,
                prisoner);
            if (activeOffer.Candidates.Count == 0)
            {
                activeOffer = null;
                return false;
            }

            for (int index = 0;
                index < offeringButtons.Length;
                index++)
            {
                Button button = offeringButtons[index];
                bool visible =
                    index < activeOffer.Candidates.Count;
                if (button == null)
                {
                    continue;
                }

                button.gameObject.SetActive(visible);
                button.onClick.RemoveAllListeners();
                if (!visible)
                {
                    continue;
                }

                OfferingCandidate candidate =
                    activeOffer.Candidates[index];
                var ownerDefinition =
                    activeOffer.Monster.MonsterDefinition;
                string ownerName = string.IsNullOrWhiteSpace(
                    ownerDefinition.display_name)
                        ? ownerDefinition.id
                        : ownerDefinition.display_name;
                BindCandidate(button, candidate, ownerName);
                button.onClick.AddListener(
                    () => Confirm(candidate.Id));
            }

            SetActive(offeringPanel, true);
            return true;
        }

        /* Offering panel과 현재 후보 상태를 닫는다. */
        public void Hide()
        {
            activeOffer = null;
            SetActive(offeringPanel, false);
        }

        /* 선택 candidate id를 Offering Service command로 확정한다. */
        private void Confirm(string candidateId)
        {
            if (!combatManager.Offerings.TryConfirm(candidateId))
            {
                return;
            }

            Hide();
            completed();
        }

        /* Offering candidate의 이름, owner, 설명을 authored button 자식에 표시한다. */
        private static void BindCandidate(
            Button button,
            OfferingCandidate candidate,
            string ownerName)
        {
            SetChildText(
                button.transform,
                "SkillName",
                ResolveLabel(candidate));
            SetChildText(button.transform, "Summary", ownerName);
            SetChildText(
                button.transform,
                "Desc",
                ResolveDescription(candidate));
        }

        /* Skill 또는 Choice candidate의 authored 표시 이름을 반환한다. */
        private static string ResolveLabel(
            OfferingCandidate candidate)
        {
            if (candidate.Skill != null)
            {
                return string.IsNullOrWhiteSpace(
                    candidate.Skill.display_name)
                        ? candidate.Id
                        : candidate.Skill.display_name;
            }

            return string.IsNullOrWhiteSpace(candidate.Choice.title)
                ? candidate.Id
                : candidate.Choice.title;
        }

        /* Skill 또는 Choice candidate의 authored 설명을 반환한다. */
        private static string ResolveDescription(
            OfferingCandidate candidate)
        {
            return candidate.Skill != null
                ? candidate.Skill.description_text
                : candidate.Choice.description_text;
        }

        /* button 자식 path의 TMP label에 안전한 text를 쓴다. */
        private static void SetChildText(
            Transform root,
            string path,
            string text)
        {
            Transform target = root.Find(path);
            TMP_Text label = target != null
                ? target.GetComponent<TMP_Text>()
                : null;
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }

        /* 현재 Canvas 아래 path에 대응하는 GameObject를 반환한다. */
        private GameObject FindObject(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.gameObject : null;
        }

        /* 현재 Canvas 아래 path에서 지정 UGUI component를 반환한다. */
        private T Find<T>(string path)
            where T : Component
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<T>() : null;
        }

        /* 선택 panel의 활성 상태를 존재하는 경우에만 바꾼다. */
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}

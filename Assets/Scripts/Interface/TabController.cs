using System;
using System.Collections;
using System.Collections.Generic;
using Bitwise.Game;
using UnityEngine;

public class TabController : MonoBehaviour
{
    public GameObject[] TabContents;

    public RectTransform TabPrefab;
    public RectTransform TabsContainer;

    private readonly List<VirtualConsoleTab> tabs = new List<VirtualConsoleTab>();

    private int activeTabIndex;

    protected void Start()
    {
        GameManager.Instance.Data.ListenForChanges(GameData.TabsVisible, OnTabsVisibleChanged);
        activeTabIndex = 0;
        for (int i = 0; i < TabContents.Length && i < 12; ++i)
        {
            string tabName = $"[F{(i + 1)}] {TabContents[i].name}";
            RectTransform tab = Instantiate(TabPrefab, TabsContainer);
            VirtualConsoleTab vTab = tab.GetComponentInChildren<VirtualConsoleTab>();
            vTab.TabLabel.text = tabName;
            vTab.Active = (i == 0);
            tabs.Add(vTab);

            TabContents[i].SetActive(i == 0);
        }
    }

    protected void OnDestroy()
    {
        GameManager.Instance.Data.StopListening(GameData.TabsVisible, OnTabsVisibleChanged);
    }

    protected void Update()
    {
        for (int i = 0; i < 12; ++i)
        {
            KeyCode keyCode = (KeyCode)(i + (int)KeyCode.F1);
            if (Input.GetKeyUp(keyCode))
            {
                SetTabActive(i);
                return;
            }
        }
    }

    private void OnTabsVisibleChanged(GameDataProperty property)
    {
        if (property.GetValue<bool>())
        {
            TabsContainer.gameObject.SetActive(true);
        }
        else
        {
            TabsContainer.gameObject.SetActive(false);
            SetTabActive(0);
        }
    }

    private void SetTabActive(int index)
    {
        if (index == activeTabIndex) { return; }

        tabs[activeTabIndex].Active = false;
        TabContents[activeTabIndex].SetActive(false);

        tabs[index].Active = true;
        TabContents[index].SetActive(true);

        activeTabIndex = index;
    }
}

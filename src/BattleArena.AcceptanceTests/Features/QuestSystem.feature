Feature: Quest System — Quest Lifecycle
    As a player
    I want to accept quests, track progress, and complete them
    So that the game world responds to my actions

    Background:
        Given a quest "The Road to Aeltharion" of type Main at level 1 rewarding 200 XP
        And a character with id 1

    @quest-lifecycle
    Scenario: Character accepts a quest
        When the character accepts the quest
        Then the quest should appear in the character's active quests

    @quest-lifecycle
    Scenario: Quest cannot be completed without being accepted
        When the character tries to complete the quest
        Then the result should be failure
        And the message should contain "not accepted"

    @quest-lifecycle
    Scenario: Quest completes when progress meets conditions
        Given the character has accepted the quest
        And the character reports progress:
            """
            {"completed": true}
            """
        When the character tries to complete the quest
        Then the result should be success

    @quest-lifecycle
    Scenario: Quest does not complete when progress is insufficient
        Given the character has accepted the quest
        And the character reports progress:
            """
            {"kills": 1, "target": 5}
            """
        When the character tries to complete the quest
        Then the result should be failure
        And the message should contain "not yet met"

    @quest-lifecycle
    Scenario: Already completed quest cannot be completed again
        Given the character has accepted the quest
        And the quest is already completed
        When the character tries to complete the quest
        Then the result should be failure
        And the message should contain "already"

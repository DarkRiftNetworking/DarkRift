Feature: ISC
	AS A game developer
	I WANT to be able to connect servers together
	SO THAT I can scale my system

@Pro
Scenario: I can connect two servers together
    Given I have a running server from ServerInGroup1.config and ClusterWithTwoGroups.config
	And I have a running server from ServerInGroup2.config and ClusterWithTwoGroups.config
	Then server 0 should synchronise to have 1 server in Group2
	And server 1 should synchronise to have 1 server in Group1
    And there are no recycling issues
    And the ServerConnected event has been fired 2 times

@Pro
Scenario Outline: I can send a message to an upstream server
    Given I have a running server from ServerInGroup1.config and ClusterWithTwoGroups.config
	And I have a running server from ServerInGroup2.config and ClusterWithTwoGroups.config
	Then server 0 should synchronise to have 1 server in Group2
    And server 1 should synchronise to have 1 server in Group1
    When server 0 sends 'Hello World' to server 1 in Group2 with tag 7 <mode>
	And server 1 has received 1 message
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| mode	     |
	| reliably   |
	| unreliably |

@Pro
Scenario Outline: I can send a message to a downstream server
    Given I have a running server from ServerInGroup1.config and ClusterWithTwoGroups.config
	And I have a running server from ServerInGroup2.config and ClusterWithTwoGroups.config
	Then server 0 should synchronise to have 1 server in Group2
    And server 1 should synchronise to have 1 server in Group1
    When server 1 sends 'Hello World' to server 0 in Group1 with tag 7 <mode>
	And server 0 has received 1 message
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| mode	     |
	| reliably   |
	| unreliably |

@Pro
Scenario: Servers can join during a scenario
    Given I have a running server from ServerInGroup1.config and ClusterWithTwoGroups.config
	And I have a running server from ServerInGroup2.config and ClusterWithTwoGroups.config
	Then server 0 should synchronise to have 1 server in Group2
    And server 1 should synchronise to have 1 server in Group1
    And the ServerConnected event has been fired 2 times
    When I have a running server from ServerInGroup2.config and ClusterWithTwoGroups.config
    Then server 0 should synchronise to have 2 servers in Group2
    And server 2 should synchronise to have 1 server in Group1
    And the ServerConnected event has been fired 4 times

@Pro
Scenario: Servers can leave during a scenario
    Given I have a running server from ServerInGroup1.config and ClusterWithTwoGroups.config
	And I have a running server from ServerInGroup2.config and ClusterWithTwoGroups.config
	Then server 0 should synchronise to have 1 server in Group2
    And server 1 should synchronise to have 1 server in Group1
    When I close server 0
    Then server 1 should synchronise to have 0 servers in Group1
    And the ServerDisconnected event has been fired 1 time

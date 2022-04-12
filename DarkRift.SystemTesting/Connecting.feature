Feature: Connecting
	AS A game developer
	I WANT to be able to connect clients to my servers
	SO THAT I can make multiplayer games

@Free
Scenario: I can connect a single client to a server
	Given I have a running server from Server.config
	And 1 client connected
	Then all clients should be connected
	And server 0 should have 1 client
    And there are no recycling issues

@Free
Scenario: I can connect multiple clients to a server
	Given I have a running server from Server.config
	And 5 clients connected
	Then all clients should be connected
	And server 0 should have 5 clients
    And there are no recycling issues

@Free
Scenario: I can disconnect from a server
	Given I have a running server from Server.config
	And 1 client connected
	Then all clients should be connected
	And server 0 should have 1 client
	When I disconnect client 0
	Then 0 clients should be connected
	And 1 client should be disconnected
	And server 0 should have 0 clients

@Free
Scenario: I receive an ID from the server
	Given I have a running server from Server.config
	And 2 clients connected
	Then all clients should be connected
	And server 0 should have 2 clients
	And client 1 has an ID of 1
    And there are no recycling issues

@Free
Scenario: I can start and stop multiple servers on the same address/port
	Given I have a running server from Server.config
	When I close and forget server 0
    Then I can start a new server from Server.config

@Free
Scenario: I can connect a single client to a server with differing UDP and TCP ports
	Given I have a running server from ServerWithDifferentUdpPort.config
	And 1 client connected
	Then all clients should be connected
	And server 0 should have 1 client

@Free
Scenario: I can connect a single client to a server over IPv6
	Given I have a running server from ServerOnIPv6.config
	And 1 client connected over IPv6
	Then all clients should be connected
	And server 0 should have 1 client
    And there are no recycling issues
    
@Free
Scenario: I can query the server's health check
	Given I have a running server from ServerWithHealthCheck.config
	When I query the health check port
	Then the server returns the expected fields

@Free
Scenario: I can start and stop multiple servers with health checks
	Given I have a running server from ServerWithHealthCheck.config
	When I close and forget server 0
    Then I can start a new server from ServerWithHealthCheck.config

@Free
Scenario: Issue #81 Being unable to connect does not stop the client from closing
    Given 1 client that fails to connect
    Then I can close client 0

Feature: Messaging
	AS A game developer
	I WANT to be able to send messages between server and client
	SO THAT I can exchange game data

@Free
Scenario Outline: I can send a message from client to server
	Given I have a running server from Server.config
	And 1 client connected
	When client 0 sends 'Hello World' with tag 5 <mode>
	And server 0 has received 1 message
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| mode	     |
	| reliably   |
	| unreliably |

@Free
Scenario Outline: I can send a message from server to client
	Given I have a running server from Server.config
	And 1 client connected
	When server 0 sends 'Hello World' to client 0 with tag 6 <mode>
	And client 0 has received 1 message
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| mode	     |
	| reliably   |
	| unreliably |

@Pro
Scenario Outline: I can get ping the from the client
	Given I have a running server from Server.config
	And the server acknowledges ping messages after 55ms
	And 1 client connected
	When client 0 sends 'Hello World' with tag 5 <mode> as a ping
	And server 0 has received 1 message
	And client 0 has received 1 message
	Then all messages are accounted for
	And client 0 has a ping of around 50ms to the server

	Examples:
	| mode	     |
	| reliably   |
	| unreliably |

@Pro
Scenario Outline: I can get the ping from the server
	Given I have a running server from Server.config
	And 1 client connected
	And client 0 acknowledges ping messages after 55ms
	When server 0 sends 'Hello World' to client 0 with tag 6 <mode> as a ping
	And client 0 has received 1 message
	And server 0 has received 1 message
	Then all messages are accounted for
	And server 0 has a ping of around 50ms to client 0

	Examples:
	| mode	     |
	| reliably   |
	| unreliably |

@Free
Scenario Outline: I can stress test client to server
	Given I have a running server from Server.config
	And <clients> clients connected
	When I stress test client to server with <messages> per client
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| clients | messages |
	| 5       | 100      |
	| 5       | 1000     |
	| 5       | 10000    |
	| 50      | 100      |
	| 50      | 1000     |
	| 50      | 10000    |
    
@Free
Scenario Outline: I can stress test client to server with dispatcher enabled on server
	Given I have a running server from Server.config
	And server 0 is using the dispatcher
	And <clients> clients connected
	When I stress test client to server with <messages> per client
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| clients | messages |
	| 5       | 100      |
	| 5       | 1000     |
	| 5       | 10000    |
	| 50      | 100      |
	| 50      | 1000     |
	| 50      | 10000    |

@Free
Scenario Outline: I can stress test server to client
	Given I have a running server from Server.config
	And <clients> clients connected
	When I stress test server to client with <messages> per client
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| clients | messages |
	| 5       | 100      |
	| 5       | 1000     |
	| 5       | 10000    |
	| 50      | 100      |
	| 50      | 1000     |
	| 50      | 10000    |
    
@Free
Scenario Outline: I can stress test server to client with dispatcher enabled on server
	Given I have a running server from Server.config
	And server 0 is using the dispatcher
	And <clients> clients connected
	When I stress test server to client with <messages> per client
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| clients | messages |
	| 5       | 100      |
	| 5       | 1000     |
	| 5       | 10000    |
	| 50      | 100      |
	| 50      | 1000     |
	| 50      | 10000    |

@Free
Scenario Outline: I can stress test both
	Given I have a running server from Server.config
	And <clients> clients connected
	When I stress test both with <messages> per client
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| clients | messages |
	| 5       | 100      |
	| 5       | 1000     |
	| 5       | 10000    |
	| 50      | 100      |
	| 50      | 1000     |
	| 50      | 10000    |
    
@Free
Scenario Outline: I can stress test both with dispatcher enabled on server
	Given I have a running server from Server.config
	And server 0 is using the dispatcher
	And <clients> clients connected
	When I stress test both with <messages> per client
	Then all messages are accounted for
    And there are no recycling issues

	Examples:
	| clients | messages |
	| 5       | 100      |
	| 5       | 1000     |
	| 5       | 10000    |
	| 50      | 100      |
	| 50      | 1000     |
	| 50      | 10000    |

# TODO add stress test with client dispatchers enabled

@Free
Scenario: Issue #75 messages being corrupted
	Given I have a running server from Server.config
	And server 0 is using the dispatcher
	And 1 client connected
	When I send 5000 messages reliably
	Then all messages are accounted for

@Free
Scenario: Partial TCP headers do not disconnect client
    Given I have a running server from Server.config
    And TCP and UDP sockets connected
    And no delay is enabled
    And the handshake has completed
    When bytes are sent via TCP 0, 0
    Then the TCP socket is connected
    When bytes are sent via TCP 0, 11
    Then the TCP socket is connected
    # TODO DR3 sort the fact that strings are little endian while DR is big endian
    When bytes are sent via TCP 0, 0, 0, 0, 0, 0, 4, 72, 0, 105, 0
    Then the TCP socket is connected
    And I receive string on the server from TCP 'Hi'
    And the TCP socket is connected
    
@Free
Scenario: Clients can send messages as soon as they are connected
    Given I have a running server from Server.config
    And a delay of 100ms when a client connects before assigning message handlers
    When a client connects and immediately sends a message
    Then all messages are accounted for

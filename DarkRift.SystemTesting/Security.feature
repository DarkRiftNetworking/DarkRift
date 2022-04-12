Feature: Security
	AS A game developer
    I WANT no exploitable vulnerablities in my server
    SO THAT users cannot cuase danage to my server

@Free
Scenario: large TCP body buffer allocations cause clients to be kicked
	Given I have a running server from Server.config
	And 1 client connected
    When client 0 sends 70000 characters with tag 6 reliably
	Then server 0 should have 0 clients
    # TODO verify strike event was called

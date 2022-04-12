Feature: Metric
	AS AN OPS team
	I WANT to monitor DarkRift and our game through industry standard tools
	SO THAT I have visibility into our servers alongside our existing infrastructure
        
@Pro
Scenario: I can query the server's Prometheus endpoint
	Given I have a running server from ServerWithPrometheusEndpoint.config
    # TODO connect a client here to get additional metrics
	When I query the Prometheus endpoint
	Then the server returns the metrics in ExpectedPrometheusMetrics.txt

@Pro
Scenario: I can start and stop multiple servers with Prometheus endpoints
	Given I have a running server from ServerWithPrometheusEndpoint.config
	When I close and forget server 0
    Then I can start a new server from ServerWithPrometheusEndpoint.config

# TODO Add a test that Prometheus exports are Culture independent #106

@Pro
Scenario: I can query the server's Prometheus endpoint for the correct client count
	Given I have a running server from ServerWithPrometheusEndpoint.config

    When I query the Prometheus endpoint
	Then the metric 'darkrift_client_manager_clients_connected' has value 0

	Given 1 client connected
	Then server 0 should have 1 client

    When I query the Prometheus endpoint
	Then the metric 'darkrift_client_manager_clients_connected' has value 1

    When I disconnect client 0
	Then server 0 should have 0 clients
    When I query the Prometheus endpoint
	Then the metric 'darkrift_client_manager_clients_connected' has value 0

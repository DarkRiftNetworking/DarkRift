# Contributing

Welcome to the DarkRift 2 open source project!

DarkRift 1 and 2 were originally written by Jamie Read. DarkRift 2 has since been open sourced under the care of Unordinal AB and the DarkRift community.

## Issues & Feature Requests Board
The [issues board](https://github.com/DarkRiftNetworking/DarkRift-Networking/issues) is a space to post any suggestions you have or any bugs you find with any DarkRift Networking component. Please make a quick search first to see if the issue isn't already represented.

Please try to follow the template provided and to make sure you add the correct labels to your issues/suggestions. Any example code or projects that showcase how to reproduce your bug or how you think your suggestion should be implemented would be very appreciated!

## Code Contributions

Anyone is welcome to submit code contributions to help make DarkRift better!

To increase the chance of having not already agreed upon contributions accepted into main, please coordinate via the issue tracker above before making a pull request.

## Tests

From now on, we will keep the main branch succeeding at all times. This means that your PR also needs to pass to merge. Unfortunately, we have a couple of very nondeterministic tests at the moment, so you will have to relaunch failed tests (if unrelated to your changes) until you are greenlit. If you based off an older failing main, please rebase to latest.

You are most welcome to add your own tests if an issue is not detected by the current test suite. You are allowed to to use the plain Visual Studio unit test framework going forward (TestClass/TestMethod) rather than SpecFlow, which is not an obvious fit in a programmer-only environment.

## Other Considerations

Mind the MPL 2.0 license (see [LICENSE.md](LICENSE.md)) that applies to most source files where stated so in the headers. To share changed source files, you might for instance either publish a full fork of this project with your changes, or just a repo with the changed files.

When engaging with this open source project and its community you agree to the code of conduct outlined in [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md).

## Where to Ask Questions

Useful Discords for questions:
* DarkRift Networking - https://discord.gg/bgKb7PG8Fh
* Unordinal - https://discord.gg/bxBaaYasH7

Questions can also be sent to [opensource@unordinal.com](mailto:opensource@unordinal.com)

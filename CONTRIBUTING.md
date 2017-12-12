Requirements:

- Windows 7 or higher (dunno if XP works)
- Visual Studio 2015 or higher

Optional:

- Resharper

Please try to follow these guidelines when contributing to this project:

- Keep your pull requests as **small** as possible: I don't want to review changes to hundreds of files
- Pull requests shouldn't tackle multiple things at the same time: Don't mix new feature X with refactoring of Y
- Pull requests must build on AppVeyor before I accept them
- A pull request should not break an existing feature
- Big features **should** be delivered in multiple pull requests (see above)
- Bugfixes require tests which reproduce the bug (unless infeasible)
- New classes / methods require tests
- Use "fluent" syntax for unit test assertions (current.Should().Be(expected))
- Assertions (see above) should declare the reason **why** this assertion is required in text-form

Getting it done:

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request :D

If you have any questions, or want to discuss an issue, just hit me up here on github, I'm here daily.

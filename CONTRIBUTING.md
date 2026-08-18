# Contributing

Thanks for helping improve BetterAutoRun.

## Development setup

1. Install Valheim and BepInExPack for Valheim.
2. Create the publicized Valheim assembly used by the existing development setup.
3. Set `VALHEIM_INSTALL` and, when needed, `BEPINEX_PATH` as described in the README.
4. Run `nuget restore BetterAutoRun.sln`.
5. Build the solution and test the plugin in a local single-player world before testing multiplayer behavior.

## Pull requests

- Keep changes focused and describe observable behavior changes.
- Preserve existing configuration keys unless a migration is included.
- Do not commit Valheim, Unity, BepInEx, or generated package binaries.
- Update the changelog for user-visible changes.
- Confirm that both Debug and Release builds succeed.

By submitting a contribution, you agree to license it under GPL-3.0-or-later, the same license used by BetterAutoRun. Preserve existing copyright and attribution notices and document user-visible changes.

## Bug reports

Include reproduction steps, expected and actual behavior, relevant versions, configuration, and a sanitized BepInEx log.

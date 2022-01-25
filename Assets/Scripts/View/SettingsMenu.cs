using System.Collections;

public class SettingsMenu : SceneSingleton<SettingsMenu>
{
    public Table TableParams;

    protected override IEnumerator OnStart()
    {
        TableParams = new Table();
        return base.OnStart();
    }
}

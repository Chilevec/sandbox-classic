[Library( "weapon_tool", Title = "Toolgun" )]
public class ToolGun : BaseWeapon
{
	[ConVar( "tool_current" )] public static string UserToolCurrent { get; set; } = "tool_boxgun";

	[Sync] public BaseTool CurrentTool { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		UpdateTool();
	}

	string lastTool;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( lastTool != UserToolCurrent )
		{
			UpdateTool();
		}
	}

	public override void AttackPrimary()
	{
		var trace = TraceTool( Owner.AimRay.Position, Owner.AimRay.Position + Owner.AimRay.Forward * 5000 );

		if ( !trace.Hit )
			return;

		if ( !(CurrentTool?.Primary( trace ) ?? false) )
			return;

		ToolEffects( trace.EndPosition );
	}

	public override void AttackSecondary()
	{
		var trace = TraceTool( Owner.AimRay.Position, Owner.AimRay.Position + Owner.AimRay.Forward * 5000 );

		if ( !trace.Hit )
			return;

		if ( !(CurrentTool?.Secondary( trace ) ?? false) )
			return;

		ToolEffects( trace.EndPosition );
	}

	public override void Reload()
	{
		var trace = TraceTool( Owner.AimRay.Position, Owner.AimRay.Position + Owner.AimRay.Forward * 5000 );

		if ( !trace.Hit )
			return;

		if ( !(CurrentTool?.Reload( trace ) ?? false) )
			return;

		ToolEffects( trace.EndPosition );
	}

	[Broadcast]
	void ToolEffects( Vector3 position )
	{
		Particles.MakeParticleSystem( "particles/tool_hit.vpcf", new Transform( position ) );
		Sound.Play( "sounds/balloon_pop_cute.sound", WorldPosition );
	}

	public void UpdateTool()
	{
		var comp = TypeLibrary.GetType( UserToolCurrent );

		if ( comp == null )
			return;

		lastTool = UserToolCurrent;

		Components.Create( comp, true );

		CurrentTool?.Destroy();

		CurrentTool = Components.Get<BaseTool>();
		CurrentTool.Parent = this;
		CurrentTool.Owner = Owner;
	}

	public SceneTraceResult TraceTool( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Scene.Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.IgnoreGameObjectHierarchy( Owner.GameObject )
				.Size( radius );

		var tr = trace.Run();

		return tr;
	}
}

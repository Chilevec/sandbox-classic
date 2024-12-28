﻿[Library( "tool_balloon", Description = "A balloon that you can turn on and off (but actually can't yet)", Group = "construction" )]
public class Balloon : BaseTool
{
	PreviewModel PreviewModel;
	RealTimeSince timeSinceDisabled;

	protected override void OnAwake()
	{
		if ( IsProxy )
			return;

		PreviewModel = new PreviewModel
		{
			ModelPath = "models/citizen_props/balloonregular01.vmdl_c",
			RotationOffset = Rotation.From( new Angles( 0, 0, 0 ) ),
			FaceNormal = false
		};
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		if ( timeSinceDisabled < Time.Delta * 5f || !Parent.IsValid() )
			return;

		var trace = Parent.BasicTraceTool();

		PreviewModel.Update( trace );
	}

	public override bool Primary( SceneTraceResult trace )
	{
		if ( !trace.Hit || !trace.GameObject.IsValid() )
			return false;

		if ( Input.Pressed( "attack1" ) )
		{
			if ( trace.Tags.Contains( "balloon" ) || trace.Tags.Contains( "player" ) )
				return true;

			var balloon = SpawnBalloon( trace );

			balloon.Components.Create<BalloonGravity>();

			PropHelper propHelper = balloon.Components.Get<PropHelper>();

			if ( !propHelper.IsValid() )
				return true;

			propHelper.Rope( trace.GameObject, Vector3.Zero, trace.GameObject.WorldTransform.PointToLocal(trace.EndPosition));

			return true;
		}

		return false;
	}

	void PositionBalloon( GameObject balloon, SceneTraceResult trace )
	{
		balloon.WorldPosition = trace.HitPosition;
	}

	protected override void OnDestroy()
	{
		PreviewModel?.Destroy();
		base.OnDestroy();
	}

	public override void Disabled()
	{
		timeSinceDisabled = 0;
		PreviewModel?.Destroy();
	}

	GameObject SpawnBalloon( SceneTraceResult trace )
	{
		var go = new GameObject()
		{
			Tags = { "solid", "balloon" }
		};

		PositionBalloon( go, trace );

		var prop = go.AddComponent<Prop>();
		prop.Model = Model.Load( "models/citizen_props/balloonregular01.vmdl_c" );

		var propHelper = go.AddComponent<PropHelper>();
		propHelper.Invincible = true;

		if ( prop.Components.TryGet<SkinnedModelRenderer>( out var renderer ) )
		{
			renderer.CreateBoneObjects = true;
		}

		var rb = propHelper.Rigidbody;
		if ( rb.IsValid() )
		{
			foreach ( var shape in rb.PhysicsBody.Shapes )
			{
				if ( !shape.IsMeshShape )
					continue;

				var newCollider = go.AddComponent<BoxCollider>();
				newCollider.Scale = prop.Model.PhysicsBounds.Size;
			}
		}

		go.NetworkSpawn();
		go.Network.SetOrphanedMode( NetworkOrphaned.Host );

		return go;
	}
}

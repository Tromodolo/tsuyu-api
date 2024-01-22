using FluentMigrator;

namespace tsuyu.Migrations;

// Turning the date colums into automatic columns instead of manually set ones
[Migration(20240122214500)]
public class AutomaticDates: Migration {
	public override void Up() {
		Alter.Table("users")
			.AlterColumn("created_at")
				.AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

		Alter.Table("files")
			.AlterColumn("created_at")
				.AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
	}

	public override void Down() {
		Alter.Table("users")
			.AlterColumn("created_at")
			.AsDateTime().NotNullable();

		Alter.Table("files")
			.AlterColumn("created_at")
			.AsDateTime().NotNullable();
	}
}

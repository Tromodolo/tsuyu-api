using FluentMigrator;

namespace tsuyu.Migrations;

// Initial database creation
[Migration(20240122210000)]
public class CreateTables: Migration {
	public override void Up() {
		Create.Table("users")
			.WithColumn("id").AsInt32().PrimaryKey().Identity()
			.WithColumn("username").AsString(64).NotNullable()
			.WithColumn("hashed_password").AsString(255).NotNullable()
			.WithColumn("email").AsString(255)
			.WithColumn("is_admin").AsBoolean().NotNullable()
			.WithColumn("api_key").AsString(255)
			.WithColumn("created_at").AsDateTime().NotNullable();

		Create.Table("files")
			.WithColumn("id").AsInt32().PrimaryKey().Identity()
			.WithColumn("name").AsString(255).NotNullable()
			.WithColumn("original_name").AsString(255).NotNullable()
			.WithColumn("filetype").AsString(64).NotNullable()
			.WithColumn("file_size").AsInt64().NotNullable()
			.WithColumn("file_hash").AsString(255).NotNullable()
			.WithColumn("uploaded_by").AsInt32().NotNullable()
			.WithColumn("uploaded_by_ip").AsString(128).NotNullable()
			.WithColumn("created_at").AsDateTime().NotNullable();

		Create.ForeignKey("files_user_id")
			.FromTable("files").ForeignColumn("uploaded_by")
			.ToTable("user").PrimaryColumn("id");
	}
	public override void Down() {
		Delete.ForeignKey("files_user_id");
		Delete.Table("files");
		Delete.Table("users");
	}
}

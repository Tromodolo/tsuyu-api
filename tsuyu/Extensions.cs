using FluentMigrator.Runner;

namespace tsuyu;

public static class Extensions {
	public static IApplicationBuilder MigrateDatabase(this IApplicationBuilder app) {
		using var scope = app.ApplicationServices.CreateScope();
		var runner = scope.ServiceProvider.GetService<IMigrationRunner>();

		runner.ListMigrations();
		runner.MigrateUp();

		return app;
	}
}

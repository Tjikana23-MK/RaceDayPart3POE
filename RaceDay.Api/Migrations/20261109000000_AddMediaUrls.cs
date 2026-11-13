using Microsoft.EntityFrameworkCore.Migrations;using Microsoft.EntityFrameworkCore.Infrastructure;using RaceDay.Api.Data;
#nullable disable
namespace RaceDay.Api.Migrations;
[DbContext(typeof(RaceDayDbContext))]
[Migration("20261109000000_AddMediaUrls")]
public partial class AddMediaUrls : Migration{
 protected override void Up(MigrationBuilder migrationBuilder){migrationBuilder.AddColumn<string>(name:"BannerUrl",table:"Events",type:"nvarchar(max)",nullable:true);}
 protected override void Down(MigrationBuilder migrationBuilder){migrationBuilder.DropColumn(name:"BannerUrl",table:"Events");}
}

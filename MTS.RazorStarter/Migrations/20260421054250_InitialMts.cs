using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTS.RazorStarter.Migrations
{
    /// <inheritdoc />
    public partial class InitialMts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "document",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    document_no = table.Column<string>(type: "TEXT", nullable: false),
                    document_type = table.Column<string>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    lifecycle_state = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    item_no = table.Column<string>(type: "TEXT", nullable: false),
                    item_type = table.Column<string>(type: "TEXT", nullable: false),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    unit_of_measure = table.Column<string>(type: "TEXT", nullable: false),
                    lifecycle_state = table.Column<string>(type: "TEXT", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "document_revision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    document_id = table.Column<int>(type: "INTEGER", nullable: false),
                    revision_code = table.Column<string>(type: "TEXT", nullable: false),
                    file_name = table.Column<string>(type: "TEXT", nullable: false),
                    file_path = table.Column<string>(type: "TEXT", nullable: false),
                    mime_type = table.Column<string>(type: "TEXT", nullable: true),
                    checksum_sha256 = table.Column<string>(type: "TEXT", nullable: true),
                    file_size_bytes = table.Column<long>(type: "INTEGER", nullable: true),
                    is_current = table.Column<bool>(type: "INTEGER", nullable: false),
                    release_state = table.Column<string>(type: "TEXT", nullable: false),
                    created_by = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_revision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_revision_document_document_id",
                        column: x => x.document_id,
                        principalTable: "document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "component_part",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "INTEGER", nullable: false),
                    make_buy = table.Column<string>(type: "TEXT", nullable: false),
                    vendor_part_no = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_component_part", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_component_part_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cut_sheet",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "INTEGER", nullable: false),
                    machine_type = table.Column<string>(type: "TEXT", nullable: true),
                    material_spec = table.Column<string>(type: "TEXT", nullable: true),
                    gauge = table.Column<string>(type: "TEXT", nullable: true),
                    sheet_size = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cut_sheet", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_cut_sheet_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "frame_assembly",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "INTEGER", nullable: false),
                    family_code = table.Column<string>(type: "TEXT", nullable: true),
                    assembly_class = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frame_assembly", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_frame_assembly_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "frame_part",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "INTEGER", nullable: false),
                    family_code = table.Column<string>(type: "TEXT", nullable: true),
                    material_spec = table.Column<string>(type: "TEXT", nullable: true),
                    default_finish = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frame_part", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_frame_part_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_revision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    item_id = table.Column<int>(type: "INTEGER", nullable: false),
                    revision_code = table.Column<string>(type: "TEXT", nullable: false),
                    revision_note = table.Column<string>(type: "TEXT", nullable: true),
                    is_current = table.Column<bool>(type: "INTEGER", nullable: false),
                    release_state = table.Column<string>(type: "TEXT", nullable: false),
                    effective_from = table.Column<DateTime>(type: "TEXT", nullable: true),
                    effective_to = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_revision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_revision_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cut_sheet_bom_line",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    cut_sheet_revision_id = table.Column<int>(type: "INTEGER", nullable: false),
                    component_item_id = table.Column<int>(type: "INTEGER", nullable: true),
                    line_no = table.Column<int>(type: "INTEGER", nullable: false),
                    part_no = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cut_sheet_bom_line", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cut_sheet_bom_line_item_component_item_id",
                        column: x => x.component_item_id,
                        principalTable: "item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_cut_sheet_bom_line_item_revision_cut_sheet_revision_id",
                        column: x => x.cut_sheet_revision_id,
                        principalTable: "item_revision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "frame_part_cut_sheet_link",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    frame_part_revision_id = table.Column<int>(type: "INTEGER", nullable: false),
                    cut_sheet_revision_id = table.Column<int>(type: "INTEGER", nullable: false),
                    usage_qty = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    link_type = table.Column<string>(type: "TEXT", nullable: false),
                    is_primary = table.Column<bool>(type: "INTEGER", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frame_part_cut_sheet_link", x => x.Id);
                    table.ForeignKey(
                        name: "FK_frame_part_cut_sheet_link_item_revision_cut_sheet_revision_id",
                        column: x => x.cut_sheet_revision_id,
                        principalTable: "item_revision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_frame_part_cut_sheet_link_item_revision_frame_part_revision_id",
                        column: x => x.frame_part_revision_id,
                        principalTable: "item_revision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item_bom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    parent_revision_id = table.Column<int>(type: "INTEGER", nullable: false),
                    child_revision_id = table.Column<int>(type: "INTEGER", nullable: false),
                    qty = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    find_no = table.Column<string>(type: "TEXT", nullable: true),
                    bom_role = table.Column<string>(type: "TEXT", nullable: false),
                    sort_order = table.Column<int>(type: "INTEGER", nullable: false),
                    notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_bom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_bom_item_revision_child_revision_id",
                        column: x => x.child_revision_id,
                        principalTable: "item_revision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_item_bom_item_revision_parent_revision_id",
                        column: x => x.parent_revision_id,
                        principalTable: "item_revision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "revision_document_link",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    item_revision_id = table.Column<int>(type: "INTEGER", nullable: false),
                    document_revision_id = table.Column<int>(type: "INTEGER", nullable: false),
                    document_role = table.Column<string>(type: "TEXT", nullable: false),
                    is_primary = table.Column<bool>(type: "INTEGER", nullable: false),
                    sort_order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revision_document_link", x => x.Id);
                    table.ForeignKey(
                        name: "FK_revision_document_link_document_revision_document_revision_id",
                        column: x => x.document_revision_id,
                        principalTable: "document_revision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_revision_document_link_item_revision_item_revision_id",
                        column: x => x.item_revision_id,
                        principalTable: "item_revision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_cut_sheet_bom_line_component_item_id",
                table: "cut_sheet_bom_line",
                column: "component_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_cut_sheet_bom_line_cut_sheet_revision_id",
                table: "cut_sheet_bom_line",
                column: "cut_sheet_revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_document_no",
                table: "document",
                column: "document_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_revision_document_id_revision_code",
                table: "document_revision",
                columns: new[] { "document_id", "revision_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_frame_part_cut_sheet_link_cut_sheet_revision_id",
                table: "frame_part_cut_sheet_link",
                column: "cut_sheet_revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_frame_part_cut_sheet_link_frame_part_revision_id_cut_sheet_revision_id_link_type",
                table: "frame_part_cut_sheet_link",
                columns: new[] { "frame_part_revision_id", "cut_sheet_revision_id", "link_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_item_no",
                table: "item",
                column: "item_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_item_bom_child_revision_id",
                table: "item_bom",
                column: "child_revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_bom_parent_revision_id",
                table: "item_bom",
                column: "parent_revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_revision_item_id_revision_code",
                table: "item_revision",
                columns: new[] { "item_id", "revision_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_revision_document_link_document_revision_id",
                table: "revision_document_link",
                column: "document_revision_id");

            migrationBuilder.CreateIndex(
                name: "IX_revision_document_link_item_revision_id_document_revision_id_document_role",
                table: "revision_document_link",
                columns: new[] { "item_revision_id", "document_revision_id", "document_role" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "component_part");

            migrationBuilder.DropTable(
                name: "cut_sheet");

            migrationBuilder.DropTable(
                name: "cut_sheet_bom_line");

            migrationBuilder.DropTable(
                name: "frame_assembly");

            migrationBuilder.DropTable(
                name: "frame_part");

            migrationBuilder.DropTable(
                name: "frame_part_cut_sheet_link");

            migrationBuilder.DropTable(
                name: "item_bom");

            migrationBuilder.DropTable(
                name: "revision_document_link");

            migrationBuilder.DropTable(
                name: "document_revision");

            migrationBuilder.DropTable(
                name: "item_revision");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "item");
        }
    }
}

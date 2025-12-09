using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public partial class AutoPulseDBCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "t_e_boitedevitesse_boi",
                schema: "public",
                columns: table => new
                {
                    boi_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    boi_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_boitedevitesse_boi", x => x.boi_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_carburant_car",
                schema: "public",
                columns: table => new
                {
                    car_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    car_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_carburant_car", x => x.car_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_categorie_cat",
                schema: "public",
                columns: table => new
                {
                    cat_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cat_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_categorie_cat", x => x.cat_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_couleur_cou",
                schema: "public",
                columns: table => new
                {
                    cou_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cou_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_couleur_cou", x => x.cou_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_etatannonce_eta",
                schema: "public",
                columns: table => new
                {
                    eta_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    eta_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_etatannonce_eta", x => x.eta_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_marque_mar",
                schema: "public",
                columns: table => new
                {
                    mar_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mar_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_marque_mar", x => x.mar_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_miseavant_mav",
                schema: "public",
                columns: table => new
                {
                    mav_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mav_libelle = table.Column<string>(type: "text", nullable: false),
                    mav_prixsemaine = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_miseavant_mav", x => x.mav_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_modeleblender_mob",
                schema: "public",
                columns: table => new
                {
                    mob_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mob_lien = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_modeleblender_mob", x => x.mob_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_motricite_mot",
                schema: "public",
                columns: table => new
                {
                    mot_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mot_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_motricite_mot", x => x.mot_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_moyenpaiement_mop",
                schema: "public",
                columns: table => new
                {
                    mop_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mop_typepaiement = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_moyenpaiement_mop", x => x.mop_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_pays_pay",
                schema: "public",
                columns: table => new
                {
                    pay_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pay_libelle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_pays_pay", x => x.pay_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_typecompte_tco",
                schema: "public",
                columns: table => new
                {
                    tco_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tco_libelle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_typecompte_tco", x => x.tco_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_typejournaux_tjo",
                schema: "public",
                columns: table => new
                {
                    tjo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tjo_libelle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_typejournaux_tjo", x => x.tjo_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_typesignalement_tsi",
                schema: "public",
                columns: table => new
                {
                    tsi_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tsi_libelle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_typesignalement_tsi", x => x.tsi_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_modele_mod",
                schema: "public",
                columns: table => new
                {
                    mod_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mod_lib = table.Column<string>(type: "text", nullable: false),
                    mar_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_modele_mod", x => x.mod_id);
                    table.ForeignKey(
                        name: "FK_t_e_modele_mod_t_e_marque_mar_mar_id",
                        column: x => x.mar_id,
                        principalSchema: "public",
                        principalTable: "t_e_marque_mar",
                        principalColumn: "mar_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_ville_vil",
                schema: "public",
                columns: table => new
                {
                    vil_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vil_libelle = table.Column<string>(type: "text", nullable: false),
                    pay_id = table.Column<int>(type: "integer", nullable: false),
                    vil_codepostal = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_ville_vil", x => x.vil_id);
                    table.ForeignKey(
                        name: "FK_t_e_ville_vil_t_e_pays_pay_pay_id",
                        column: x => x.pay_id,
                        principalSchema: "public",
                        principalTable: "t_e_pays_pay",
                        principalColumn: "pay_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_compte_com",
                schema: "public",
                columns: table => new
                {
                    com_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_pseudo = table.Column<string>(type: "text", nullable: false),
                    com_mdp = table.Column<string>(type: "text", nullable: false),
                    com_nom = table.Column<string>(type: "text", nullable: false),
                    com_prenom = table.Column<string>(type: "text", nullable: false),
                    com_email = table.Column<string>(type: "text", nullable: false),
                    com_date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    com_date_derniere_connexion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    com_date_naissance = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    com_biographie = table.Column<string>(type: "text", nullable: true),
                    tco_id = table.Column<int>(type: "integer", nullable: false),
                    cpr_siret = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    cpr_raison_sociale = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_compte_com", x => x.com_id);
                    table.ForeignKey(
                        name: "FK_t_e_compte_com_t_e_typecompte_tco_tco_id",
                        column: x => x.tco_id,
                        principalSchema: "public",
                        principalTable: "t_e_typecompte_tco",
                        principalColumn: "tco_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_voiture_voi",
                schema: "public",
                columns: table => new
                {
                    voi_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mar_id = table.Column<int>(type: "integer", nullable: false),
                    mot_id = table.Column<int>(type: "integer", nullable: false),
                    car_id = table.Column<int>(type: "integer", nullable: false),
                    boi_id = table.Column<int>(type: "integer", nullable: false),
                    cou_id = table.Column<int>(type: "integer", nullable: false),
                    cat_id = table.Column<int>(type: "integer", nullable: false),
                    voi_nbplace = table.Column<int>(type: "integer", nullable: false),
                    voi_nbporte = table.Column<int>(type: "integer", nullable: false),
                    voi_kilometrage = table.Column<int>(type: "integer", nullable: false),
                    voi_annee = table.Column<int>(type: "integer", nullable: false),
                    voi_puissance = table.Column<int>(type: "integer", nullable: false),
                    voi_couple = table.Column<int>(type: "integer", nullable: false),
                    voi_nbcylindres = table.Column<int>(type: "integer", nullable: false),
                    mob_id = table.Column<int>(type: "integer", nullable: true),
                    voi_miseencirculation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    mod_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_voiture_voi", x => x.voi_id);
                    table.ForeignKey(
                        name: "FK_t_e_voiture_voi_t_e_boitedevitesse_boi_boi_id",
                        column: x => x.boi_id,
                        principalSchema: "public",
                        principalTable: "t_e_boitedevitesse_boi",
                        principalColumn: "boi_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_voiture_voi_t_e_carburant_car_car_id",
                        column: x => x.car_id,
                        principalSchema: "public",
                        principalTable: "t_e_carburant_car",
                        principalColumn: "car_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_voiture_voi_t_e_categorie_cat_cat_id",
                        column: x => x.cat_id,
                        principalSchema: "public",
                        principalTable: "t_e_categorie_cat",
                        principalColumn: "cat_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_voiture_voi_t_e_marque_mar_mar_id",
                        column: x => x.mar_id,
                        principalSchema: "public",
                        principalTable: "t_e_marque_mar",
                        principalColumn: "mar_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_voiture_voi_t_e_modele_mod_mod_id",
                        column: x => x.mod_id,
                        principalSchema: "public",
                        principalTable: "t_e_modele_mod",
                        principalColumn: "mod_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_voiture_voi_t_e_modeleblender_mob_mob_id",
                        column: x => x.mob_id,
                        principalSchema: "public",
                        principalTable: "t_e_modeleblender_mob",
                        principalColumn: "mob_id");
                    table.ForeignKey(
                        name: "FK_t_e_voiture_voi_t_e_motricite_mot_mot_id",
                        column: x => x.mot_id,
                        principalSchema: "public",
                        principalTable: "t_e_motricite_mot",
                        principalColumn: "mot_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_adresse_adr",
                schema: "public",
                columns: table => new
                {
                    adr_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    adr_numero = table.Column<int>(type: "integer", nullable: false),
                    adr_rue = table.Column<string>(type: "text", nullable: false),
                    vil_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_adresse_adr", x => x.adr_id);
                    table.ForeignKey(
                        name: "FK_t_e_adresse_adr_t_e_ville_vil_vil_id",
                        column: x => x.vil_id,
                        principalSchema: "public",
                        principalTable: "t_e_ville_vil",
                        principalColumn: "vil_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_journal_jou",
                schema: "public",
                columns: table => new
                {
                    jou_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    jou_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    jou_contenu = table.Column<string>(type: "text", nullable: false),
                    tjo_id = table.Column<int>(type: "integer", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    IdTypeJournaux = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_journal_jou", x => x.jou_id);
                    table.ForeignKey(
                        name: "FK_t_e_journal_jou_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_journal_jou_t_e_typejournaux_tjo_tjo_id",
                        column: x => x.tjo_id,
                        principalSchema: "public",
                        principalTable: "t_e_typejournaux_tjo",
                        principalColumn: "tjo_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_signalement_sig",
                schema: "public",
                columns: table => new
                {
                    sig_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sig_description = table.Column<string>(type: "text", nullable: false),
                    sig_datecreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    com_id_signalant = table.Column<int>(type: "integer", nullable: false),
                    com_idsignale = table.Column<int>(type: "integer", nullable: false),
                    tsi_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_signalement_sig", x => x.sig_id);
                    table.ForeignKey(
                        name: "FK_t_e_signalement_sig_t_e_compte_com_com_id_signalant",
                        column: x => x.com_id_signalant,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignale",
                        column: x => x.com_idsignale,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_signalement_sig_t_e_typesignalement_tsi_tsi_id",
                        column: x => x.tsi_id,
                        principalSchema: "public",
                        principalTable: "t_e_typesignalement_tsi",
                        principalColumn: "tsi_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_image_img",
                schema: "public",
                columns: table => new
                {
                    img_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    img_fichier = table.Column<byte[]>(type: "bytea", nullable: false),
                    voi_id = table.Column<int>(type: "integer", nullable: true),
                    com_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_image_img", x => x.img_id);
                    table.ForeignKey(
                        name: "FK_t_e_image_img_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id");
                    table.ForeignKey(
                        name: "FK_t_e_image_img_t_e_voiture_voi_voi_id",
                        column: x => x.voi_id,
                        principalSchema: "public",
                        principalTable: "t_e_voiture_voi",
                        principalColumn: "voi_id");
                });

            migrationBuilder.CreateTable(
                name: "t_j_apourcouleur_apc",
                schema: "public",
                columns: table => new
                {
                    cou_id = table.Column<int>(type: "integer", nullable: false),
                    voi_id = table.Column<int>(type: "integer", nullable: false),
                    voi_idcouleur = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_j_apourcouleur_apc", x => new { x.cou_id, x.voi_id });
                    table.ForeignKey(
                        name: "FK_t_j_apourcouleur_apc_t_e_couleur_cou_cou_id",
                        column: x => x.cou_id,
                        principalSchema: "public",
                        principalTable: "t_e_couleur_cou",
                        principalColumn: "cou_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_j_apourcouleur_apc_t_e_voiture_voi_voi_id",
                        column: x => x.voi_id,
                        principalSchema: "public",
                        principalTable: "t_e_voiture_voi",
                        principalColumn: "voi_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_annonce_ann",
                schema: "public",
                columns: table => new
                {
                    ann_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ann_nom = table.Column<string>(type: "text", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    eta_id = table.Column<int>(type: "integer", nullable: false),
                    adr_id = table.Column<int>(type: "integer", nullable: false),
                    voi_id = table.Column<int>(type: "integer", nullable: false),
                    mav_id = table.Column<int>(type: "integer", nullable: true),
                    ann_dat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_annonce_ann", x => x.ann_id);
                    table.ForeignKey(
                        name: "FK_t_e_annonce_ann_t_e_adresse_adr_adr_id",
                        column: x => x.adr_id,
                        principalSchema: "public",
                        principalTable: "t_e_adresse_adr",
                        principalColumn: "adr_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_annonce_ann_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_annonce_ann_t_e_etatannonce_eta_eta_id",
                        column: x => x.eta_id,
                        principalSchema: "public",
                        principalTable: "t_e_etatannonce_eta",
                        principalColumn: "eta_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_annonce_ann_t_e_miseavant_mav_mav_id",
                        column: x => x.mav_id,
                        principalSchema: "public",
                        principalTable: "t_e_miseavant_mav",
                        principalColumn: "mav_id");
                    table.ForeignKey(
                        name: "FK_t_e_annonce_ann_t_e_voiture_voi_voi_id",
                        column: x => x.voi_id,
                        principalSchema: "public",
                        principalTable: "t_e_voiture_voi",
                        principalColumn: "voi_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_j_apouradresse_apa",
                schema: "public",
                columns: table => new
                {
                    adr_id = table.Column<int>(type: "integer", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_j_apouradresse_apa", x => new { x.adr_id, x.com_id });
                    table.ForeignKey(
                        name: "FK_t_j_apouradresse_apa_t_e_adresse_adr_adr_id",
                        column: x => x.adr_id,
                        principalSchema: "public",
                        principalTable: "t_e_adresse_adr",
                        principalColumn: "adr_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_j_apouradresse_apa_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_commande_cmd",
                schema: "public",
                columns: table => new
                {
                    cmd_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fac_id = table.Column<int>(type: "integer", nullable: true),
                    com_id_vendeur = table.Column<int>(type: "integer", nullable: false),
                    com_id_acheteur = table.Column<int>(type: "integer", nullable: false),
                    cmd_id_annonce = table.Column<int>(type: "integer", nullable: false),
                    cmd_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cmd_moyen_paiement = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_commande_cmd", x => x.cmd_id);
                    table.ForeignKey(
                        name: "FK_t_e_commande_cmd_t_e_annonce_ann_cmd_id_annonce",
                        column: x => x.cmd_id_annonce,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_commande_cmd_t_e_moyenpaiement_mop_cmd_moyen_paiement",
                        column: x => x.cmd_moyen_paiement,
                        principalSchema: "public",
                        principalTable: "t_e_moyenpaiement_mop",
                        principalColumn: "mop_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_conversation_con",
                schema: "public",
                columns: table => new
                {
                    con_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ann_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_conversation_con", x => x.con_id);
                    table.ForeignKey(
                        name: "FK_t_e_conversation_con_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_j_favori_fav",
                schema: "public",
                columns: table => new
                {
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    ann_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_j_favori_fav", x => new { x.ann_id, x.com_id });
                    table.ForeignKey(
                        name: "FK_t_j_favori_fav_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_j_favori_fav_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_avis_avi",
                schema: "public",
                columns: table => new
                {
                    avi_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_id_jugee = table.Column<int>(type: "integer", nullable: false),
                    com_id_jugeur = table.Column<int>(type: "integer", nullable: false),
                    cmd_idcommande = table.Column<int>(type: "integer", nullable: false),
                    avi_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    avi_libelle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    avi_note = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_avis_avi", x => x.avi_id);
                    table.ForeignKey(
                        name: "FK_t_e_avis_avi_t_e_commande_cmd_cmd_idcommande",
                        column: x => x.cmd_idcommande,
                        principalSchema: "public",
                        principalTable: "t_e_commande_cmd",
                        principalColumn: "cmd_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_avis_avi_t_e_compte_com_com_id_jugee",
                        column: x => x.com_id_jugee,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_avis_avi_t_e_compte_com_com_id_jugeur",
                        column: x => x.com_id_jugeur,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_facture_fac",
                schema: "public",
                columns: table => new
                {
                    fac_id = table.Column<int>(type: "integer", nullable: false),
                    cmd_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_facture_fac", x => x.fac_id);
                    table.ForeignKey(
                        name: "FK_t_e_facture_fac_t_e_commande_cmd_fac_id",
                        column: x => x.fac_id,
                        principalSchema: "public",
                        principalTable: "t_e_commande_cmd",
                        principalColumn: "cmd_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_message_mes",
                schema: "public",
                columns: table => new
                {
                    mes_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mes_contenu = table.Column<string>(type: "text", nullable: false),
                    mes_dateenvoi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    con_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_message_mes", x => x.mes_id);
                    table.ForeignKey(
                        name: "FK_t_e_message_mes_t_e_conversation_con_con_id",
                        column: x => x.con_id,
                        principalSchema: "public",
                        principalTable: "t_e_conversation_con",
                        principalColumn: "con_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_j_apourconversation_apc",
                schema: "public",
                columns: table => new
                {
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    con_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_j_apourconversation_apc", x => new { x.com_id, x.con_id });
                    table.ForeignKey(
                        name: "FK_t_j_apourconversation_apc_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_j_apourconversation_apc_t_e_conversation_con_con_id",
                        column: x => x.con_id,
                        principalSchema: "public",
                        principalTable: "t_e_conversation_con",
                        principalColumn: "con_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_adresse_adr_vil_id",
                schema: "public",
                table: "t_e_adresse_adr",
                column: "vil_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_annonce_ann_adr_id",
                schema: "public",
                table: "t_e_annonce_ann",
                column: "adr_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_annonce_ann_com_id",
                schema: "public",
                table: "t_e_annonce_ann",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_annonce_ann_eta_id_ann_dat",
                schema: "public",
                table: "t_e_annonce_ann",
                columns: new[] { "eta_id", "ann_dat" });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_annonce_ann_mav_id",
                schema: "public",
                table: "t_e_annonce_ann",
                column: "mav_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_annonce_ann_voi_id",
                schema: "public",
                table: "t_e_annonce_ann",
                column: "voi_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_avis_avi_cmd_idcommande",
                schema: "public",
                table: "t_e_avis_avi",
                column: "cmd_idcommande");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_avis_avi_com_id_jugee",
                schema: "public",
                table: "t_e_avis_avi",
                column: "com_id_jugee");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_avis_avi_com_id_jugeur",
                schema: "public",
                table: "t_e_avis_avi",
                column: "com_id_jugeur");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_commande_cmd_cmd_id_annonce",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "cmd_id_annonce");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_commande_cmd_cmd_moyen_paiement",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "cmd_moyen_paiement");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_commande_cmd_com_id_acheteur",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "com_id_acheteur");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_commande_cmd_com_id_vendeur",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "com_id_vendeur");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_compte_com_com_email",
                schema: "public",
                table: "t_e_compte_com",
                column: "com_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_e_compte_com_com_pseudo",
                schema: "public",
                table: "t_e_compte_com",
                column: "com_pseudo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_e_compte_com_tco_id",
                schema: "public",
                table: "t_e_compte_com",
                column: "tco_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_conversation_con_ann_id",
                schema: "public",
                table: "t_e_conversation_con",
                column: "ann_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_image_img_com_id",
                schema: "public",
                table: "t_e_image_img",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_image_img_voi_id",
                schema: "public",
                table: "t_e_image_img",
                column: "voi_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_journal_jou_com_id_jou_date",
                schema: "public",
                table: "t_e_journal_jou",
                columns: new[] { "com_id", "jou_date" });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_journal_jou_tjo_id",
                schema: "public",
                table: "t_e_journal_jou",
                column: "tjo_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_message_mes_con_id",
                schema: "public",
                table: "t_e_message_mes",
                column: "con_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_message_mes_mes_id_mes_dateenvoi",
                schema: "public",
                table: "t_e_message_mes",
                columns: new[] { "mes_id", "mes_dateenvoi" });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_modele_mod_mar_id",
                schema: "public",
                table: "t_e_modele_mod",
                column: "mar_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_com_id_signalant",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "com_id_signalant");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_com_idsignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "com_idsignale");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_tsi_id",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "tsi_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_ville_vil_pay_id",
                schema: "public",
                table: "t_e_ville_vil",
                column: "pay_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_ville_vil_vil_codepostal",
                schema: "public",
                table: "t_e_ville_vil",
                column: "vil_codepostal");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_voiture_voi_boi_id",
                schema: "public",
                table: "t_e_voiture_voi",
                column: "boi_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_voiture_voi_car_id",
                schema: "public",
                table: "t_e_voiture_voi",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_voiture_voi_cat_id",
                schema: "public",
                table: "t_e_voiture_voi",
                column: "cat_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_voiture_voi_mar_id",
                schema: "public",
                table: "t_e_voiture_voi",
                column: "mar_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_voiture_voi_mob_id",
                schema: "public",
                table: "t_e_voiture_voi",
                column: "mob_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_voiture_voi_mod_id",
                schema: "public",
                table: "t_e_voiture_voi",
                column: "mod_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_voiture_voi_mot_id",
                schema: "public",
                table: "t_e_voiture_voi",
                column: "mot_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_j_apouradresse_apa_com_id",
                schema: "public",
                table: "t_j_apouradresse_apa",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_j_apourconversation_apc_con_id",
                schema: "public",
                table: "t_j_apourconversation_apc",
                column: "con_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_j_apourcouleur_apc_voi_id",
                schema: "public",
                table: "t_j_apourcouleur_apc",
                column: "voi_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_j_favori_fav_com_id",
                schema: "public",
                table: "t_j_favori_fav",
                column: "com_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_e_avis_avi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_facture_fac",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_image_img",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_journal_jou",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_message_mes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_signalement_sig",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_apouradresse_apa",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_apourconversation_apc",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_apourcouleur_apc",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_favori_fav",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_commande_cmd",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_typejournaux_tjo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_typesignalement_tsi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_conversation_con",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_couleur_cou",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_moyenpaiement_mop",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_annonce_ann",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_adresse_adr",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_compte_com",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_etatannonce_eta",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_miseavant_mav",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_voiture_voi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_ville_vil",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_typecompte_tco",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_boitedevitesse_boi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_carburant_car",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_categorie_cat",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_modele_mod",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_modeleblender_mob",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_motricite_mot",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_pays_pay",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_marque_mar",
                schema: "public");
        }
    }
}

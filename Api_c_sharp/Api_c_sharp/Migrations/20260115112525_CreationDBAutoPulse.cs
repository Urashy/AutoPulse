using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api_c_sharp.Migrations
{
    /// <inheritdoc />
    public partial class CreationDBAutoPulse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "t_e_benchmarksia_ben",
                schema: "public",
                columns: table => new
                {
                    ben_id_benchmark = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ben_benchmark_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ben_model_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ben_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ben_total_iterations = table.Column<int>(type: "integer", nullable: false),
                    ben_successful_predictions = table.Column<int>(type: "integer", nullable: false),
                    ben_failed_predictions = table.Column<int>(type: "integer", nullable: false),
                    ben_success_rate_percent = table.Column<double>(type: "double precision", precision: 5, scale: 2, nullable: false),
                    ben_avg_inference_time_ms = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    ben_min_inference_time_ms = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    ben_max_inference_time_ms = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    ben_std_inference_time_ms = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    ben_predictions_per_second = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    ben_total_time_seconds = table.Column<double>(type: "double precision", precision: 18, scale: 6, nullable: false),
                    ben_platform = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ben_processor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ben_python_version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ben_cpu_count = table.Column<int>(type: "integer", nullable: true),
                    ben_memory_total_gb = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    ben_memory_available_gb = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_benchmarksia_ben", x => x.ben_id_benchmark);
                });

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
                    cou_lib = table.Column<string>(type: "text", nullable: false),
                    cou_codehexa = table.Column<string>(type: "text", nullable: false)
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
                name: "t_e_etatcommande_etc",
                schema: "public",
                columns: table => new
                {
                    etc_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    etc_libelle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_etatcommande_etc", x => x.etc_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_etatcompte_etc",
                schema: "public",
                columns: table => new
                {
                    etc_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    etc_libelle = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_etatcompte_etc", x => x.etc_id);
                });

            migrationBuilder.CreateTable(
                name: "t_e_etatsignalementplainte_ets",
                schema: "public",
                columns: table => new
                {
                    ets_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    eta_lib = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_etatsignalementplainte_ets", x => x.ets_id);
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
                    tco_libelle = table.Column<string>(type: "text", nullable: false),
                    tco_cherchable = table.Column<bool>(type: "boolean", nullable: false)
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
                    com_numero_telephone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    etc_id = table.Column<int>(type: "integer", nullable: false),
                    cpr_siret = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    cpr_raison_sociale = table.Column<string>(type: "text", nullable: true),
                    com_google_id = table.Column<string>(type: "text", nullable: true),
                    com_auth_provider = table.Column<string>(type: "text", nullable: true),
                    com_a2f_actif = table.Column<bool>(type: "boolean", nullable: false),
                    com_date_derniere_activation_a2f = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    com_email_verif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_compte_com", x => x.com_id);
                    table.ForeignKey(
                        name: "FK_t_e_compte_com_t_e_etatcompte_etc_etc_id",
                        column: x => x.etc_id,
                        principalSchema: "public",
                        principalTable: "t_e_etatcompte_etc",
                        principalColumn: "etc_id",
                        onDelete: ReferentialAction.Cascade);
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
                    cat_id = table.Column<int>(type: "integer", nullable: false),
                    voi_nbplace = table.Column<int>(type: "integer", nullable: false),
                    voi_nbporte = table.Column<int>(type: "integer", nullable: false),
                    voi_kilometrage = table.Column<int>(type: "integer", nullable: false),
                    voi_annee = table.Column<int>(type: "integer", nullable: false),
                    voi_puissance = table.Column<int>(type: "integer", nullable: false),
                    voi_couple = table.Column<int>(type: "integer", nullable: false),
                    voi_nbcylindres = table.Column<int>(type: "integer", nullable: false),
                    voi_nbairbag = table.Column<int>(type: "integer", nullable: false),
                    voi_interieurcuire = table.Column<bool>(type: "boolean", nullable: false),
                    voi_cylindrermoteur = table.Column<double>(type: "double precision", nullable: false),
                    voi_positionvolant = table.Column<bool>(type: "boolean", nullable: false),
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
                    adr_nom = table.Column<string>(type: "text", nullable: false),
                    adr_numero = table.Column<int>(type: "integer", nullable: false),
                    adr_rue = table.Column<string>(type: "text", nullable: false),
                    adr_libelleville = table.Column<string>(type: "text", nullable: false),
                    adr_codepostal = table.Column<string>(type: "text", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    pays_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_adresse_adr", x => x.adr_id);
                    table.ForeignKey(
                        name: "FK_t_e_adresse_adr_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_adresse_adr_t_e_pays_pay_pays_id",
                        column: x => x.pays_id,
                        principalSchema: "public",
                        principalTable: "t_e_pays_pay",
                        principalColumn: "pay_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_cartebancaire_cba",
                schema: "public",
                columns: table => new
                {
                    cba_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cba_numero = table.Column<string>(type: "text", nullable: false),
                    cba_code_securite = table.Column<string>(type: "text", nullable: false),
                    cba_date_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    com_idcompte = table.Column<int>(type: "integer", nullable: false),
                    cba_typecarte = table.Column<string>(type: "text", nullable: false),
                    cba_nomcarte = table.Column<string>(type: "text", nullable: true),
                    cba_nomtitulaire = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_cartebancaire_cba", x => x.cba_id);
                    table.ForeignKey(
                        name: "FK_t_e_cartebancaire_cba_t_e_compte_com_com_idcompte",
                        column: x => x.com_idcompte,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
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
                name: "t_e_refresh_token_rft",
                schema: "public",
                columns: table => new
                {
                    rft_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rft_token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    rft_date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rft_date_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rft_est_revoque = table.Column<bool>(type: "boolean", nullable: false),
                    rft_date_revocation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rft_remember_me = table.Column<bool>(type: "boolean", nullable: false),
                    rft_ip_creation = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    rft_user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_refresh_token_rft", x => x.rft_id);
                    table.ForeignKey(
                        name: "FK_t_e_refresh_token_rft_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_tokenemail_tke",
                schema: "public",
                columns: table => new
                {
                    tke_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_id = table.Column<int>(type: "integer", nullable: true),
                    com_email = table.Column<string>(type: "text", nullable: false),
                    tke_token = table.Column<string>(type: "text", nullable: false),
                    tke_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tke_utilise = table.Column<bool>(type: "boolean", nullable: false),
                    tke_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_tokenemail_tke", x => x.tke_id);
                    table.ForeignKey(
                        name: "FK_t_e_tokenemail_tke_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id");
                });

            migrationBuilder.CreateTable(
                name: "t_j_bloque_blo",
                schema: "public",
                columns: table => new
                {
                    blo_id = table.Column<int>(type: "integer", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    blo_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_j_bloque_blo", x => new { x.com_id, x.blo_id });
                    table.ForeignKey(
                        name: "FK_t_j_bloque_blo_t_e_compte_com_blo_id",
                        column: x => x.blo_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_j_bloque_blo_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
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
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_image_img_t_e_voiture_voi_voi_id",
                        column: x => x.voi_id,
                        principalSchema: "public",
                        principalTable: "t_e_voiture_voi",
                        principalColumn: "voi_id",
                        onDelete: ReferentialAction.Cascade);
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
                    mav_id = table.Column<int>(type: "integer", nullable: false),
                    ann_dat = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ann_pri = table.Column<int>(type: "integer", nullable: false),
                    ann_description = table.Column<string>(type: "text", nullable: true),
                    ann_prochainemiseenavant = table.Column<int>(type: "integer", nullable: true)
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
                        principalColumn: "mav_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_annonce_ann_t_e_voiture_voi_voi_id",
                        column: x => x.voi_id,
                        principalSchema: "public",
                        principalTable: "t_e_voiture_voi",
                        principalColumn: "voi_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_conversation_con",
                schema: "public",
                columns: table => new
                {
                    con_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ann_id = table.Column<int>(type: "integer", nullable: false),
                    con_date_dernier_message = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "t_e_notification_not",
                schema: "public",
                columns: table => new
                {
                    not_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    not_titre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    not_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    not_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    not_url_navigation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    not_est_lue = table.Column<bool>(type: "boolean", nullable: false),
                    not_date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ann_id = table.Column<int>(type: "integer", nullable: true),
                    not_ancien_prix = table.Column<double>(type: "double precision", nullable: true),
                    not_nouveau_prix = table.Column<double>(type: "double precision", nullable: true),
                    not_pseudo_acheteur_offre = table.Column<string>(type: "text", nullable: true),
                    not_valeur_offre = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_notification_not", x => x.not_id);
                    table.ForeignKey(
                        name: "FK_t_e_notification_not_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id");
                    table.ForeignKey(
                        name: "FK_t_e_notification_not_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
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
                    com_idsignalant = table.Column<int>(type: "integer", nullable: false),
                    com_idsignale = table.Column<int>(type: "integer", nullable: true),
                    ann_idannoncesignale = table.Column<int>(type: "integer", nullable: true),
                    tsi_id = table.Column<int>(type: "integer", nullable: false),
                    ets_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_signalement_sig", x => x.sig_id);
                    table.ForeignKey(
                        name: "FK_t_e_signalement_sig_t_e_annonce_ann_ann_idannoncesignale",
                        column: x => x.ann_idannoncesignale,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id");
                    table.ForeignKey(
                        name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignalant",
                        column: x => x.com_idsignalant,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_signalement_sig_t_e_compte_com_com_idsignale",
                        column: x => x.com_idsignale,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id");
                    table.ForeignKey(
                        name: "FK_t_e_signalement_sig_t_e_etatsignalementplainte_ets_ets_id",
                        column: x => x.ets_id,
                        principalSchema: "public",
                        principalTable: "t_e_etatsignalementplainte_ets",
                        principalColumn: "ets_id",
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
                name: "t_j_vue_vue",
                schema: "public",
                columns: table => new
                {
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    ann_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_j_vue_vue", x => new { x.com_id, x.ann_id });
                    table.ForeignKey(
                        name: "FK_t_j_vue_vue_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_j_vue_vue_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
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
                    con_id = table.Column<int>(type: "integer", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    mes_estlu = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_message_mes", x => x.mes_id);
                    table.ForeignKey(
                        name: "FK_t_e_message_mes_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "t_e_plainte_pla",
                schema: "public",
                columns: table => new
                {
                    pla_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pla_description = table.Column<string>(type: "text", nullable: false),
                    pla_date_creation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sig_id = table.Column<int>(type: "integer", nullable: false),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    ets_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_plainte_pla", x => x.pla_id);
                    table.ForeignKey(
                        name: "FK_t_e_plainte_pla_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_plainte_pla_t_e_etatsignalementplainte_ets_ets_id",
                        column: x => x.ets_id,
                        principalSchema: "public",
                        principalTable: "t_e_etatsignalementplainte_ets",
                        principalColumn: "ets_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_plainte_pla_t_e_signalement_sig_sig_id",
                        column: x => x.sig_id,
                        principalSchema: "public",
                        principalTable: "t_e_signalement_sig",
                        principalColumn: "sig_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_offre_off",
                schema: "public",
                columns: table => new
                {
                    off_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ann_id = table.Column<int>(type: "integer", nullable: false),
                    mes_id = table.Column<int>(type: "integer", nullable: false),
                    off_valeur = table.Column<decimal>(type: "numeric", nullable: false),
                    off_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    off_estaccepte = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_offre_off", x => x.off_id);
                    table.ForeignKey(
                        name: "FK_t_e_offre_off_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_offre_off_t_e_message_mes_mes_id",
                        column: x => x.mes_id,
                        principalSchema: "public",
                        principalTable: "t_e_message_mes",
                        principalColumn: "mes_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_piecejointe_pj",
                schema: "public",
                columns: table => new
                {
                    pj_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    mes_id = table.Column<int>(type: "integer", nullable: false),
                    pj_nom_fichier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    pj_type_mime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pj_extension = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    pj_taille_fichier = table.Column<long>(type: "bigint", nullable: false),
                    pj_contenu = table.Column<byte[]>(type: "bytea", nullable: false),
                    pj_date_upload = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_piecejointe_pj", x => x.pj_id);
                    table.ForeignKey(
                        name: "FK_t_e_piecejointe_pj_t_e_message_mes_mes_id",
                        column: x => x.mes_id,
                        principalSchema: "public",
                        principalTable: "t_e_message_mes",
                        principalColumn: "mes_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_commande_cmd",
                schema: "public",
                columns: table => new
                {
                    cmd_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    com_id_vendeur = table.Column<int>(type: "integer", nullable: false),
                    com_id_acheteur = table.Column<int>(type: "integer", nullable: false),
                    cmd_id_annonce = table.Column<int>(type: "integer", nullable: false),
                    cmd_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    etc_id = table.Column<int>(type: "integer", nullable: false),
                    moy_moyenpaiement = table.Column<int>(type: "integer", nullable: false),
                    off_idoffre = table.Column<int>(type: "integer", nullable: false)
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
                        name: "FK_t_e_commande_cmd_t_e_compte_com_com_id_acheteur",
                        column: x => x.com_id_acheteur,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_commande_cmd_t_e_compte_com_com_id_vendeur",
                        column: x => x.com_id_vendeur,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_commande_cmd_t_e_etatcommande_etc_etc_id",
                        column: x => x.etc_id,
                        principalSchema: "public",
                        principalTable: "t_e_etatcommande_etc",
                        principalColumn: "etc_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_commande_cmd_t_e_moyenpaiement_mop_moy_moyenpaiement",
                        column: x => x.moy_moyenpaiement,
                        principalSchema: "public",
                        principalTable: "t_e_moyenpaiement_mop",
                        principalColumn: "mop_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_commande_cmd_t_e_offre_off_off_idoffre",
                        column: x => x.off_idoffre,
                        principalSchema: "public",
                        principalTable: "t_e_offre_off",
                        principalColumn: "off_id",
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
                    fac_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cmd_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_facture_fac", x => x.fac_id);
                    table.ForeignKey(
                        name: "FK_t_e_facture_fac_t_e_commande_cmd_cmd_id",
                        column: x => x.cmd_id,
                        principalSchema: "public",
                        principalTable: "t_e_commande_cmd",
                        principalColumn: "cmd_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "t_e_paiement_pmt",
                schema: "public",
                columns: table => new
                {
                    pmt_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ann_id = table.Column<int>(type: "integer", nullable: false),
                    cba_id = table.Column<int>(type: "integer", nullable: true),
                    mav_id = table.Column<int>(type: "integer", nullable: true),
                    cmd_id = table.Column<int>(type: "integer", nullable: true),
                    com_id = table.Column<int>(type: "integer", nullable: false),
                    pmpt_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_e_paiement_pmt", x => x.pmt_id);
                    table.ForeignKey(
                        name: "FK_t_e_paiement_pmt_t_e_annonce_ann_ann_id",
                        column: x => x.ann_id,
                        principalSchema: "public",
                        principalTable: "t_e_annonce_ann",
                        principalColumn: "ann_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_paiement_pmt_t_e_cartebancaire_cba_cba_id",
                        column: x => x.cba_id,
                        principalSchema: "public",
                        principalTable: "t_e_cartebancaire_cba",
                        principalColumn: "cba_id");
                    table.ForeignKey(
                        name: "FK_t_e_paiement_pmt_t_e_commande_cmd_cmd_id",
                        column: x => x.cmd_id,
                        principalSchema: "public",
                        principalTable: "t_e_commande_cmd",
                        principalColumn: "cmd_id");
                    table.ForeignKey(
                        name: "FK_t_e_paiement_pmt_t_e_compte_com_com_id",
                        column: x => x.com_id,
                        principalSchema: "public",
                        principalTable: "t_e_compte_com",
                        principalColumn: "com_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_t_e_paiement_pmt_t_e_miseavant_mav_mav_id",
                        column: x => x.mav_id,
                        principalSchema: "public",
                        principalTable: "t_e_miseavant_mav",
                        principalColumn: "mav_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_adresse_adr_com_id",
                schema: "public",
                table: "t_e_adresse_adr",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_adresse_adr_pays_id",
                schema: "public",
                table: "t_e_adresse_adr",
                column: "pays_id");

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
                name: "IX_t_e_benchmarksia_ben_ben_benchmark_id",
                schema: "public",
                table: "t_e_benchmarksia_ben",
                column: "ben_benchmark_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_benchmarksia_ben_ben_model_type_ben_timestamp",
                schema: "public",
                table: "t_e_benchmarksia_ben",
                columns: new[] { "ben_model_type", "ben_timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_t_e_cartebancaire_cba_com_idcompte",
                schema: "public",
                table: "t_e_cartebancaire_cba",
                column: "com_idcompte");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_commande_cmd_cmd_id_annonce",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "cmd_id_annonce");

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
                name: "IX_t_e_commande_cmd_etc_id",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "etc_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_commande_cmd_moy_moyenpaiement",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "moy_moyenpaiement");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_commande_cmd_off_idoffre",
                schema: "public",
                table: "t_e_commande_cmd",
                column: "off_idoffre");

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
                name: "IX_t_e_compte_com_etc_id",
                schema: "public",
                table: "t_e_compte_com",
                column: "etc_id");

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
                name: "IX_t_e_facture_fac_cmd_id",
                schema: "public",
                table: "t_e_facture_fac",
                column: "cmd_id");

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
                name: "IX_t_e_message_mes_com_id",
                schema: "public",
                table: "t_e_message_mes",
                column: "com_id");

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
                name: "IX_t_e_notification_not_ann_id",
                schema: "public",
                table: "t_e_notification_not",
                column: "ann_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_notification_not_com_id",
                schema: "public",
                table: "t_e_notification_not",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_offre_off_ann_id",
                schema: "public",
                table: "t_e_offre_off",
                column: "ann_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_offre_off_mes_id",
                schema: "public",
                table: "t_e_offre_off",
                column: "mes_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_paiement_pmt_ann_id",
                schema: "public",
                table: "t_e_paiement_pmt",
                column: "ann_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_paiement_pmt_cba_id",
                schema: "public",
                table: "t_e_paiement_pmt",
                column: "cba_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_paiement_pmt_cmd_id",
                schema: "public",
                table: "t_e_paiement_pmt",
                column: "cmd_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_paiement_pmt_com_id",
                schema: "public",
                table: "t_e_paiement_pmt",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_paiement_pmt_mav_id",
                schema: "public",
                table: "t_e_paiement_pmt",
                column: "mav_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_piecejointe_pj_mes_id",
                schema: "public",
                table: "t_e_piecejointe_pj",
                column: "mes_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_plainte_pla_com_id",
                schema: "public",
                table: "t_e_plainte_pla",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_plainte_pla_ets_id",
                schema: "public",
                table: "t_e_plainte_pla",
                column: "ets_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_plainte_pla_sig_id",
                schema: "public",
                table: "t_e_plainte_pla",
                column: "sig_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_refresh_token_rft_com_id",
                schema: "public",
                table: "t_e_refresh_token_rft",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_ann_idannoncesignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ann_idannoncesignale");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_com_idsignalant",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "com_idsignalant");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_com_idsignale",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "com_idsignale");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_ets_id",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "ets_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_signalement_sig_tsi_id",
                schema: "public",
                table: "t_e_signalement_sig",
                column: "tsi_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_e_tokenemail_tke_com_id",
                schema: "public",
                table: "t_e_tokenemail_tke",
                column: "com_id");

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
                name: "IX_t_j_bloque_blo_blo_id",
                schema: "public",
                table: "t_j_bloque_blo",
                column: "blo_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_j_favori_fav_com_id",
                schema: "public",
                table: "t_j_favori_fav",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_t_j_vue_vue_ann_id",
                schema: "public",
                table: "t_j_vue_vue",
                column: "ann_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_e_avis_avi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_benchmarksia_ben",
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
                name: "t_e_notification_not",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_paiement_pmt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_piecejointe_pj",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_plainte_pla",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_refresh_token_rft",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_tokenemail_tke",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_apourconversation_apc",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_apourcouleur_apc",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_bloque_blo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_favori_fav",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_j_vue_vue",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_typejournaux_tjo",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_cartebancaire_cba",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_commande_cmd",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_signalement_sig",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_couleur_cou",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_etatcommande_etc",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_moyenpaiement_mop",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_offre_off",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_etatsignalementplainte_ets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_typesignalement_tsi",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_message_mes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_conversation_con",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_annonce_ann",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_adresse_adr",
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
                name: "t_e_compte_com",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_pays_pay",
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
                name: "t_e_etatcompte_etc",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_typecompte_tco",
                schema: "public");

            migrationBuilder.DropTable(
                name: "t_e_marque_mar",
                schema: "public");
        }
    }
}

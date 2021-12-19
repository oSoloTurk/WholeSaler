--
-- PostgreSQL database dump
--

-- Dumped from database version 14.0
-- Dumped by pg_dump version 14.0

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: public; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO postgres;

--
-- Name: logItemChange(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logItemChange"() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
	 DECLARE BEGIN
	 INSERT INTO "Actions" ("EffecterUser", "Date", "ActionDescription", "ActionElement") VALUES (NEW."LastModifier", CURRENT_TIMESTAMP, CONCAT('Name: ', OLD."ItemName", '->', NEW."ItemName", E'\r\n', 'Price: ', OLD."ItemPrice", '->', NEW."ItemPrice", E'\r\n', 'Description: ', OLD."ItemDesc", '->', NEW."ItemDesc"), 'Items');
	 RETURN NEW;
	END; 
	$$;


ALTER FUNCTION public."logItemChange"() OWNER TO postgres;

--
-- Name: logLocationChange(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logLocationChange"() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
	 DECLARE BEGIN
     INSERT INTO "Actions" ("EffecterUser", "Date", "ActionDescription", "ActionElement") VALUES (NEW."LastModifier", CURRENT_TIMESTAMP, CONCAT('Old Adress: ', OLD."Adress", ' (CityID: ' , OLD."CityID" , ')') || E'\r\n' || CONCAT('New Adress: ', NEW."Adress", ' (CityID: ' , NEW."CityID" , ')') , 'Locations');
	 RETURN NEW;
	END; 
	$$;


ALTER FUNCTION public."logLocationChange"() OWNER TO postgres;

--
-- Name: logOperationStart(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logOperationStart"() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
	 DECLARE BEGIN
	 if NEW."VehicleID" is not null then
        INSERT INTO "Actions" ("EffecterUser", "Date", "ActionDescription", "ActionElement") VALUES (NEW."LastModifier", CURRENT_TIMESTAMP, CONCAT('Operation ', NEW."OperationID" , ' Started with vehicle ', NEW."VehicleID", ' by ', NEW."LastModifier"), 'Operations');
     END IF;
	 RETURN NEW;
	END; 
	$$;


ALTER FUNCTION public."logOperationStart"() OWNER TO postgres;

--
-- Name: logVehicleChange(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."logVehicleChange"() RETURNS trigger
    LANGUAGE plpgsql
    AS $$
	 DECLARE BEGIN
	 INSERT INTO "Actions" ("EffecterUser", "Date", "ActionDescription", "ActionElement") VALUES (NEW."LastModifier", CURRENT_TIMESTAMP, CONCAT(OLD."VehicleName", '->', NEW."VehicleName"), 'Vehicles');
	 RETURN NEW;
	END; 
	$$;


ALTER FUNCTION public."logVehicleChange"() OWNER TO postgres;

--
-- Name: requestcity(text, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.requestcity(cityname text, countryid integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    cityId int;
    upperCityName text DEFAULT UPPER(cityName);
BEGIN
    IF EXISTS (Select * FROM "City" WHERE "NormalizedCityName" = upperCityName) THEN
        UPDATE "City" SET "RequestCounter" = ("RequestCounter" + 1) WHERE "NormalizedCityName" = upperCityName;
    ELSE         
        INSERT INTO "City" ("CityName", "OperationalState", "RequestCounter", "CountryID", "NormalizedCityName") VALUES (cityName, false, 1, countryId, UPPER(cityName));
        cityId:=currval('city_cityid_seq');
    END IF;
    RETURN cityId;
END;
$$;


ALTER FUNCTION public.requestcity(cityname text, countryid integer) OWNER TO postgres;

--
-- Name: requestcountry(text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.requestcountry(countryname text) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    countryId int;
    upperCountryName text DEFAULT UPPER(countryName);
BEGIN
    IF EXISTS (Select * FROM "Country" WHERE "NormalizedCountryName" = upperCountryName) THEN
        UPDATE "Country" SET "RequestCounter" = ("RequestCounter" + 1) WHERE "NormalizedCountryName" = upperCountryName RETURNING "CountryID" into countryId;
    ELSE 
        INSERT INTO "Country" ("CountryName", "OperationalState", "RequestCounter", "NormalizedCountryName") VALUES (countryName, false, 1, upperCountryName);
        countryId := currval('country_countryid_seq');
    END IF;
    return countryId;
END;
$$;


ALTER FUNCTION public.requestcountry(countryname text) OWNER TO postgres;

--
-- Name: requestoperationalstate(text, text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.requestoperationalstate(cityname text, countryname text) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    countryId int;
BEGIN
    countryId := requestCountry(countryName::TEXT);
    countryId := requestCity(cityName::TEXT, countryId::INT);
END;
$$;


ALTER FUNCTION public.requestoperationalstate(cityname text, countryname text) OWNER TO postgres;

--
-- Name: endOperation(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public."endOperation"(vehicleid integer) RETURNS boolean
    LANGUAGE plpgsql
    AS $$
     DECLARE     
        basketId INT;
     BEGIN
     basketId := (Select "BasketID" from "Operations" where "VehicleID" = vehicleId);
     if basketId IS not null then
        UPDATE "Operations" Set "VehicleID" = null WHERE "VehicleID" = vehicleId;
        update "Basket" set "IsArchived" = true where "BasketID" = basketId;
        RETURN TRUE;
     end if;
     RETURN FALSE;
    END; 
    $$;


ALTER FUNCTION public."endOperation"(vehicleid integer) OWNER TO postgres;


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Actions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Actions" (
    "ActionID" integer NOT NULL,
    "EffecterUser" text NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "ActionDescription" text NOT NULL,
    "ActionElement" text
);


ALTER TABLE public."Actions" OWNER TO postgres;

--
-- Name: Actions_ActionID_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Actions" ALTER COLUMN "ActionID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Actions_ActionID_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Alerts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Alerts" (
    "AlertID" integer NOT NULL,
    "UserID" text NOT NULL,
    "Message" text NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Redirect" text NOT NULL
);


ALTER TABLE public."Alerts" OWNER TO postgres;

--
-- Name: AspNetRoleClaims; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetRoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


ALTER TABLE public."AspNetRoleClaims" OWNER TO postgres;

--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."AspNetRoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetRoleClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetRoles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetRoles" (
    "Id" text NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text
);


ALTER TABLE public."AspNetRoles" OWNER TO postgres;

--
-- Name: AspNetUserClaims; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUserClaims" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


ALTER TABLE public."AspNetUserClaims" OWNER TO postgres;

--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."AspNetUserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetUserClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetUserLogins; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUserLogins" (
    "LoginProvider" character varying(128) NOT NULL,
    "ProviderKey" character varying(128) NOT NULL,
    "ProviderDisplayName" text,
    "UserId" text NOT NULL
);


ALTER TABLE public."AspNetUserLogins" OWNER TO postgres;

--
-- Name: AspNetUserRoles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL
);


ALTER TABLE public."AspNetUserRoles" OWNER TO postgres;

--
-- Name: AspNetUserTokens; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" character varying(128) NOT NULL,
    "Name" character varying(128) NOT NULL,
    "Value" text
);


ALTER TABLE public."AspNetUserTokens" OWNER TO postgres;

--
-- Name: AspNetUsers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUsers" (
    "Id" text NOT NULL,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL,
    "CompanyName" character varying(256),
    "Name" character varying(256),
    "SurName" character varying(256)
);


ALTER TABLE public."AspNetUsers" OWNER TO postgres;

--
-- Name: Basket; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Basket" (
    "BasketID" integer NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "UserID" text NOT NULL,
    "IsArchived" boolean NOT NULL
);


ALTER TABLE public."Basket" OWNER TO postgres;

--
-- Name: Basket_items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Basket_items" (
    "BasketID" integer NOT NULL,
    "ItemID" integer NOT NULL,
    "Amount" integer DEFAULT 1,
    "BasketItemID" integer NOT NULL,
    "BasketPrice" real NOT NULL
);


ALTER TABLE public."Basket_items" OWNER TO postgres;

--
-- Name: Category; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Category" (
    "CategoryID" integer NOT NULL,
    "CategoryName" text
);


ALTER TABLE public."Category" OWNER TO postgres;

--
-- Name: City; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."City" (
    "CityID" integer NOT NULL,
    "CityName" text NOT NULL,
    "CountryID" integer NOT NULL,
    "OperationalState" boolean,
    "RequestCounter" integer DEFAULT 0,
    "NormalizedCityName" text
);


ALTER TABLE public."City" OWNER TO postgres;

--
-- Name: Country; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Country" (
    "CountryID" integer NOT NULL,
    "CountryName" text NOT NULL,
    "OperationalState" boolean,
    "RequestCounter" integer DEFAULT 0,
    "NormalizedCountryName" text
);


ALTER TABLE public."Country" OWNER TO postgres;

--
-- Name: Items; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Items" (
    "ItemID" integer NOT NULL,
    "ItemName" text,
    "ItemPrice" integer,
    "ItemDesc" text,
    "CategoryID" integer,
    "LastModifier" text
);


ALTER TABLE public."Items" OWNER TO postgres;

--
-- Name: Items_ItemID_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Items" ALTER COLUMN "ItemID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Items_ItemID_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Locations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Locations" (
    "LocationID" integer NOT NULL,
    "LocationOwnerID" text NOT NULL,
    "Adress" text,
    "CityID" integer,
    "LastModifier" text
);


ALTER TABLE public."Locations" OWNER TO postgres;

--
-- Name: Locations_LocationID_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Locations" ALTER COLUMN "LocationID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Locations_LocationID_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Operations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Operations" (
    "OperationID" integer NOT NULL,
    "OperationValue" integer NOT NULL,
    "Date" timestamp with time zone,
    "BasketID" integer,
    "LocationID" integer,
    "VehicleID" integer,
    "OwnerID" text,
    "LastModifier" text
);


ALTER TABLE public."Operations" OWNER TO postgres;

--
-- Name: Operations_OperationID_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Operations" ALTER COLUMN "OperationID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Operations_OperationID_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Vehicles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Vehicles" (
    "VehicleID" integer NOT NULL,
    "VehicleName" text,
    "VehiclePlate" text,
    "LastModifier" text
);


ALTER TABLE public."Vehicles" OWNER TO postgres;

--
-- Name: Vehicles_VehicleID_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Vehicles" ALTER COLUMN "VehicleID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public."Vehicles_VehicleID_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: alerts_alertid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Alerts" ALTER COLUMN "AlertID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.alerts_alertid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: basket_basketid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Basket" ALTER COLUMN "BasketID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.basket_basketid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: basket_items_basketitemid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Basket_items" ALTER COLUMN "BasketItemID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.basket_items_basketitemid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: category_categoryid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Category" ALTER COLUMN "CategoryID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.category_categoryid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: city_cityid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."City" ALTER COLUMN "CityID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.city_cityid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: country_countryid_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Country" ALTER COLUMN "CountryID" ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.country_countryid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: Actions; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Actions" ("ActionID", "EffecterUser", "Date", "ActionDescription", "ActionElement") OVERRIDING SYSTEM VALUE VALUES
	(25, 'Hakki', '2021-12-15 00:00:00+03', 'Old Adress:  (CityID: )
New Adress: Kemalpaşa Esentepe Kampüsü, Üniversite Cd. (CityID: 175)', 'Locations'),
	(26, 'Hakki', '2021-12-15 00:00:00+03', 'Operation 12 Started with vehicle 1 by Hakki', 'Operations'),
	(27, 'Hakki', '2021-12-15 00:00:00+03', 'Operation 13 Started with vehicle 2 by Hakki', 'Operations'),
	(29, 'Hakki', '2021-12-15 14:34:21.298275+03', 'Operation 15 Started with vehicle 2 by Hakki', 'Operations'),
	(30, 'Hakki', '2021-12-15 17:31:39.955735+03', 'Operation 13 Started with vehicle 4 by Hakki', 'Operations'),
	(31, 'Hakki', '2021-12-15 17:33:09.074784+03', 'Operation 12 Started with vehicle 1 by Hakki', 'Operations'),
	(32, 'Hakki', '2021-12-15 17:33:15.784334+03', 'Operation 14 Started with vehicle 2 by Hakki', 'Operations'),
	(33, 'Hakki', '2021-12-15 19:07:02.63768+03', 'Operation 14 Started with vehicle 2 by Hakki', 'Operations'),
	(34, 'Hakki', '2021-12-19 14:13:13.285824+03', '->Ford 2642 HR', 'Vehicles'),
	(35, 'Hakki', '2021-12-19 14:13:46.543272+03', '->Ford 1848T Midilli', 'Vehicles'),
	(36, 'Hakki', '2021-12-19 14:14:39.812886+03', '->Ford 1842', 'Vehicles'),
	(37, 'Hakki', '2021-12-19 14:15:30.629328+03', 'Old Adress: Kemalpaşa Esentepe Kampüsü, Üniversite Cd. (CityID: 175)
New Adress: Kemalpaşa Esentepe Kampüsü, Üniversite Cd. (Bilişim Sistemleri Fakültesi) (CityID: 175)', 'Locations'),
	(38, 'Hakki', '2021-12-19 14:16:05.905601+03', 'Operation 19 Started with vehicle 6 by Hakki', 'Operations');


--
-- Data for Name: Alerts; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Alerts" ("AlertID", "UserID", "Message", "Date", "Redirect") OVERRIDING SYSTEM VALUE VALUES
	(29, '18749dc4-0c10-4a42-bc44-23f5b9c512b9', 'We got your orders we will send notification to you when sending you.', '2021-12-19 14:14:55.82014+03', '/Dashboard/UserBoard'),
	(30, '18749dc4-0c10-4a42-bc44-23f5b9c512b9', 'We send your orders!', '2021-12-19 14:16:05.901818+03', '/Dashboard/UserBoard');


--
-- Data for Name: AspNetRoleClaims; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: AspNetRoles; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") VALUES
	('c456e194-ab82-43e8-a5a3-d692c8dfb6c4', 'SuperAdmin', 'SUPERADMIN', '7e40ebcd-53d3-40e1-a57e-39ad46990b83'),
	('2ccccc44-fd68-4a0c-a1e1-685762ec695a', 'Admin', 'ADMIN', '07d8d36c-07e1-4107-bf0e-0d2d71b19b24'),
	('af324fbd-1a79-42b0-8ae6-47f2402e8a22', 'Employee', 'EMPLOYEE', '1ff34a48-4e00-4785-95b7-90cb06929af4'),
	('53f8ea13-2a40-43b4-bd0f-991f6959aa7d', 'Customer', 'CUSTOMER', '60df2894-72a7-4b01-89d2-48e6e02d3ad2');


--
-- Data for Name: AspNetUserClaims; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: AspNetUserLogins; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: AspNetUserRoles; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."AspNetUserRoles" ("UserId", "RoleId") VALUES
	('c6208001-2da3-4bcf-ab7c-8c7bed17c9fb', '53f8ea13-2a40-43b4-bd0f-991f6959aa7d'),
	('c6208001-2da3-4bcf-ab7c-8c7bed17c9fb', 'af324fbd-1a79-42b0-8ae6-47f2402e8a22'),
	('c6208001-2da3-4bcf-ab7c-8c7bed17c9fb', '2ccccc44-fd68-4a0c-a1e1-685762ec695a'),
	('c6208001-2da3-4bcf-ab7c-8c7bed17c9fb', 'c456e194-ab82-43e8-a5a3-d692c8dfb6c4'),
	('18749dc4-0c10-4a42-bc44-23f5b9c512b9', 'c456e194-ab82-43e8-a5a3-d692c8dfb6c4'),
	('18749dc4-0c10-4a42-bc44-23f5b9c512b9', '2ccccc44-fd68-4a0c-a1e1-685762ec695a'),
	('18749dc4-0c10-4a42-bc44-23f5b9c512b9', 'af324fbd-1a79-42b0-8ae6-47f2402e8a22'),
	('18749dc4-0c10-4a42-bc44-23f5b9c512b9', '53f8ea13-2a40-43b4-bd0f-991f6959aa7d');


--
-- Data for Name: AspNetUserTokens; Type: TABLE DATA; Schema: public; Owner: postgres
--



--
-- Data for Name: AspNetUsers; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount", "CompanyName", "Name", "SurName") VALUES
	('18749dc4-0c10-4a42-bc44-23f5b9c512b9', 'Hakki', 'HAKKI', 'g211210350@sakarya.edu.tr', 'G211210350@SAKARYA.EDU.TR', true, 'AQAAAAEAACcQAAAAED1EVbgftSD8DPXCo7Nt/QlRpniZxWh3cc3SWW8gWcwqdgcxSC0eguutMDfowmJn+w==', 'W2NIBNYMOZFJ4QHRRYUQIQWCBRF5UHXP', 'f84887f7-6c3d-4372-89d8-bc5c572fcfec', NULL, true, false, NULL, true, 0, NULL, 'Hakki', 'Ceylan'),
	('c6208001-2da3-4bcf-ab7c-8c7bed17c9fb', 'Mete', 'METE', 'g191210053@sakarya.edu.tr', 'G191210053@SAKARYA.EDU.TR', true, 'AQAAAAEAACcQAAAAEMenir7LgtOmOE06hUiNNjwbmpl55aBC5I3z58GBo9kupIlDagR4OPMPzO82gPOd3w==', 'JHPL6VZGCFCVNPXROQBBAW7L4KGAU2LC', 'fbe49326-0c6c-420d-a15a-ca4633a3c713', NULL, true, false, NULL, true, 0, NULL, 'Mete', 'Dokgöz');


--
-- Data for Name: Basket; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Basket" ("BasketID", "Date", "UserID", "IsArchived") OVERRIDING SYSTEM VALUE VALUES
	(11, '2021-12-19 14:14:46.896897+03', '18749dc4-0c10-4a42-bc44-23f5b9c512b9', true);


--
-- Data for Name: Basket_items; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Basket_items" ("BasketID", "ItemID", "Amount", "BasketItemID", "BasketPrice") OVERRIDING SYSTEM VALUE VALUES
	(11, 33, 3, 77, 429),
	(11, 34, 4, 78, 1800),
	(11, 36, 1, 79, 13);


--
-- Data for Name: Category; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Category" ("CategoryID", "CategoryName") OVERRIDING SYSTEM VALUE VALUES
	(2, 'Electronics'),
	(3, 'Home and Appliances'),
	(4, 'Mix'),
	(5, 'Office  Products'),
	(6, 'Pharmacy 
 and Health');


--
-- Data for Name: City; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."City" ("CityID", "CityName", "CountryID", "OperationalState", "RequestCounter", "NormalizedCityName") OVERRIDING SYSTEM VALUE VALUES
	(180, 'Girne', 4, true, 0, 'GIRNE'),
	(181, 'Gönyeli', 4, true, 0, 'GÖNYELI'),
	(182, 'Güzelyurt', 4, true, 0, 'GÜZELYURT'),
	(183, 'Lapta', 4, true, 0, 'LAPTA'),
	(184, 'Alsancak', 4, true, 0, 'ALSANCAK'),
	(185, 'Hamitköy', 4, true, 0, 'HAMITKÖY'),
	(186, 'Çatalköy', 4, true, 0, 'ÇATALKÖY'),
	(97, 'Ankara', 2, true, 0, 'ANKARA'),
	(98, 'Edirne', 2, true, 0, 'EDIRNE'),
	(99, 'Çanakkale', 2, true, 0, 'ÇANAKKALE'),
	(101, 'Balıkesir', 2, true, 0, 'BALıKESIR'),
	(102, 'Aydın', 2, true, 0, 'AYDıN'),
	(103, 'Muğla', 2, true, 0, 'MUĞLA'),
	(104, 'Afyonkarahisar', 2, true, 0, 'AFYONKARAHISAR'),
	(105, 'Bilecik', 2, true, 0, 'BILECIK'),
	(106, 'Burdur', 2, true, 0, 'BURDUR'),
	(107, 'Düzce', 2, true, 0, 'DÜZCE'),
	(108, 'Eskişehir', 2, true, 0, 'ESKIŞEHIR'),
	(109, 'Konya', 2, true, 0, 'KONYA'),
	(110, 'Kütahya', 2, true, 0, 'KÜTAHYA'),
	(111, 'Manisa', 2, true, 0, 'MANISA'),
	(112, 'Uşak', 2, true, 0, 'UŞAK'),
	(113, 'Yalova', 2, true, 0, 'YALOVA'),
	(114, 'Adana', 2, true, 0, 'ADANA'),
	(115, 'Hakkâri', 2, true, 0, 'HAKKÂRI'),
	(116, 'Van', 2, true, 0, 'VAN'),
	(117, 'Iğdır', 2, true, 0, 'IĞDıR'),
	(118, 'Antakya', 2, true, 0, 'ANTAKYA'),
	(119, 'Kahramanmaraş', 2, true, 0, 'KAHRAMANMARAŞ'),
	(120, 'Mersin', 2, true, 0, 'MERSIN'),
	(121, 'Osmaniye', 2, true, 0, 'OSMANIYE'),
	(122, 'Aksaray', 2, true, 0, 'AKSARAY'),
	(123, 'Karaman', 2, true, 0, 'KARAMAN'),
	(124, 'Kayseri', 2, true, 0, 'KAYSERI'),
	(125, 'Kırıkkale', 2, true, 0, 'KıRıKKALE'),
	(126, 'Kırşehir', 2, true, 0, 'KıRŞEHIR'),
	(127, 'Nevşehir', 2, true, 0, 'NEVŞEHIR'),
	(128, 'Niğde', 2, true, 0, 'NIĞDE'),
	(129, 'Sivas', 2, true, 0, 'SIVAS'),
	(130, 'Yozgat', 2, true, 0, 'YOZGAT'),
	(131, 'Amasya', 2, true, 0, 'AMASYA'),
	(132, 'Artvin', 2, true, 0, 'ARTVIN'),
	(133, 'Bartın', 2, true, 0, 'BARTıN'),
	(134, 'Bayburt', 2, true, 0, 'BAYBURT'),
	(135, 'Çorum', 2, true, 0, 'ÇORUM'),
	(136, 'Giresun', 2, true, 0, 'GIRESUN'),
	(137, 'Gümüşhane', 2, true, 0, 'GÜMÜŞHANE'),
	(138, 'Karabük', 2, true, 0, 'KARABÜK'),
	(139, 'Kastamonu', 2, true, 0, 'KASTAMONU'),
	(140, 'Ordu', 2, true, 0, 'ORDU'),
	(141, 'Rize', 2, true, 0, 'RIZE'),
	(142, 'Samsun', 2, true, 0, 'SAMSUN'),
	(143, 'Sinop', 2, true, 0, 'SINOP'),
	(144, 'Tokat', 2, true, 0, 'TOKAT'),
	(145, 'Zonguldak', 2, true, 0, 'ZONGULDAK'),
	(146, 'Ağrı', 2, true, 0, 'AĞRı'),
	(147, 'Ardahan', 2, true, 0, 'ARDAHAN'),
	(148, 'Bingöl', 2, true, 0, 'BINGÖL'),
	(149, 'Bitlis', 2, true, 0, 'BITLIS'),
	(150, 'Elazığ', 2, true, 0, 'ELAZıĞ'),
	(151, 'Muş', 2, true, 0, 'MUŞ'),
	(152, 'Erzincan', 2, true, 0, 'ERZINCAN'),
	(153, 'Erzurum', 2, true, 0, 'ERZURUM'),
	(154, 'Kars', 2, true, 0, 'KARS'),
	(155, 'Malatya', 2, true, 0, 'MALATYA'),
	(156, 'Tunceli', 2, true, 0, 'TUNCELI'),
	(157, 'Adıyaman', 2, true, 0, 'ADıYAMAN'),
	(158, 'Batman', 2, true, 0, 'BATMAN'),
	(159, 'Diyarbakır', 2, true, 0, 'DIYARBAKıR'),
	(160, 'Gaziantep', 2, true, 0, 'GAZIANTEP'),
	(161, 'Kilis', 2, true, 0, 'KILIS'),
	(162, 'Mardin', 2, true, 0, 'MARDIN'),
	(163, 'Şanlıurfa', 2, true, 0, 'ŞANLıURFA'),
	(164, 'Siirt', 2, true, 0, 'SIIRT'),
	(165, 'Şırnak', 2, true, 0, 'ŞıRNAK'),
	(166, 'Isparta', 2, true, 0, 'ISPARTA'),
	(167, 'Bursa', 2, true, 0, 'BURSA'),
	(168, 'İzmir', 2, true, 0, 'İZMIR'),
	(169, 'Antalya', 2, true, 0, 'ANTALYA'),
	(170, 'Tekirdağ', 2, true, 0, 'TEKIRDAĞ'),
	(171, 'İstanbul', 2, true, 0, 'İSTANBUL'),
	(172, 'Bolu', 2, true, 0, 'BOLU'),
	(173, 'Bergama', 2, true, 0, 'BERGAMA'),
	(174, 'Çankırı', 2, true, 0, 'ÇANKıRı'),
	(175, 'Sakarya', 2, true, 0, 'SAKARYA'),
	(176, 'İzmit', 2, true, 0, 'İZMIT'),
	(177, 'Denizli', 2, true, 0, 'DENIZLI'),
	(100, 'Kırklareli', 2, false, 1, 'KıRKLARELI');


--
-- Data for Name: Country; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Country" ("CountryID", "CountryName", "OperationalState", "RequestCounter", "NormalizedCountryName") OVERRIDING SYSTEM VALUE VALUES
	(4, 'Kuzey Kıbrıs Türk Cumhuriyeti', true, 0, 'KUZEY KıBRıS TÜRK CUMHURIYETI'),
	(2, 'Türkiye', true, 1, 'TÜRKIYE');


--
-- Data for Name: Items; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Items" ("ItemID", "ItemName", "ItemPrice", "ItemDesc", "CategoryID", "LastModifier") OVERRIDING SYSTEM VALUE VALUES
	(7, 'Samsung BD-J5900 Curved 3D Blu-ray Player with Wi-Fi (2015 Model)', 80, NULL, 4, NULL),
	(8, 'Roku 3 Streaming Media Player (4230R) with Voice Search (2015 model)', 90, NULL, 4, NULL),
	(9, 'Char-Broil Patio Bistro Cover', 3, NULL, 4, NULL),
	(10, 'Le Creuset Signature Enameled Cast-Iron 5-1/2-Quart Round French (Dutch) Oven. Cherry', 300, NULL, 4, NULL),
	(11, 'Oster BLSTPB-WPK My Blend 250-Watt Blender with Travel Sport Bottle. Pink', 20, NULL, 5, NULL),
	(13, 'Philips Sonicare HX6042/60 Sonicare for Kids Brush Heads. Ages 7-10. 2-Pack', 9, NULL, 4, NULL),
	(14, 'Philips Sonicare HX6042/64 Sonicare for Kids Replacement Brush Heads. Ages 7-10. 2 Pack', 9, NULL, 4, NULL),
	(15, 'Philips Sonicare DiamondClean Sonic Electric Toothbrush. Black. HX9352/04', 190, NULL, 4, NULL),
	(16, '3M 07493 Surface Conditioning Disc Pad', 4, NULL, 5, NULL),
	(17, 'Post-it Super Sticky Pop-up Notes. Colors of the World Collection. 3 in x 3 in. New York (R330-6SSNY)', 7, NULL, 5, NULL),
	(18, 'Circo® Wild Horses Decorative Pillow', 14, NULL, 4, NULL),
	(19, 'Disney Frozen Sparkle Princess Anna Doll', 31, NULL, 4, NULL),
	(20, 'Nordic Ware Natural Aluminum Commercial Bakers Half Sheet', 9, NULL, 4, NULL),
	(21, 'Brita Hard Sided Water Filter Bottle. Grey. 23.7 Ounces', 12, NULL, 4, NULL),
	(22, 'Emerson Compact Refrigerator', 130, NULL, 4, NULL),
	(23, 'Igloo 1.7 cu ft Refrigerator', 70, NULL, 4, NULL),
	(24, 'Crock-Pot SCR200-B Manual Slow Cooker. 2 Quart', 10, NULL, 4, NULL),
	(25, 'Crock-Pot SCCPVG000 18-Ounce Electric Gravy Warmer. White', 12, NULL, 4, NULL),
	(26, 'Avery Big Tab Insertable Plastic Dividers. 8-Tabs. 1 Set (11901)', 3, NULL, 5, NULL),
	(27, 'Pendaflex SureHook Reinforced Hanging Folders. Letter Size. Standard Green. 20 per Box (6152 1/5)', 18, NULL, 5, NULL),
	(28, 'Paw Patrol Toddler Bed Set. Blue', 37, NULL, 4, NULL),
	(29, 'EAS Myoplex Original Ready-to-Drink Nutrition Shake. Chocolate Fudge. 1.06 PT.. 4 Count (Pack Of 3)', 10, NULL, 4, NULL),
	(30, 'Larabar Gluten Free Fruit & Nut Food Bar. Peanut Butter Chocolate Chip. 16 - 1.6 Ounce Bars', 8, NULL, 4, NULL),
	(31, 'Logitech Speakers Z130', 24, NULL, 5, NULL),
	(32, 'Logitech Speakers Z130', 15, NULL, 5, NULL),
	(33, 'Logitech G910 Orion Spark RGB Mechanical Gaming Keyboard (920-006385)', 143, NULL, 2, NULL),
	(34, 'Canon EOS Rebel T6 Digital SLR Camera Kit with EF-S 18-55mm and EF 75-300mm Zoom Lenses (Black)', 450, NULL, 2, NULL),
	(35, 'American Standard 2034.014.020 Champion-4 Right Height One-Piece Elongated Toilet. White', 199, NULL, 3, NULL),
	(36, '550VA 6 Outlet UPS 550VA 6 Outlet UPS', 13, NULL, 2, NULL),
	(37, '550VA 6 Outlet UPS 550VA 6 Outlet UPS', 12, NULL, 2, NULL),
	(38, 'Texas Instruments BA II Plus Financial Calculator', 32, NULL, 2, NULL),
	(39, 'Cobra 2 SPX 5500 Ultra-High Performance Radar/Laser Detector', 128, NULL, 2, NULL),
	(40, 'Turtle Beach - Ear Force XO Seven Pro Premium Gaming Headset - Superhuman Hearing - Xbox One', 107, NULL, 2, NULL),
	(41, 'Sanus Systems ELM701B1 Anti-Tip Strap for Flat Panel TV or Furniture', 17, NULL, 2, NULL),
	(42, 'Troy-Bilt TB516 EC 29cc 4-Cycle Wheeled Edger with JumpStart Technology', 179, NULL, 3, NULL),
	(43, 'Ekena Millwork COR05X04X17ST 5 7/8-Inch W x 4 1/4-Inch D x 17 3/4-Inch H Stockport Corbel', 1, NULL, 6, NULL),
	(44, 'Chamberlain MYQ-G0201 MyQ-Garage Controls Your Garage Door Opener with Your Smartphone', 100, NULL, 2, NULL),
	(45, 'WD 4TB My Book Desktop External Hard Drive - USB 3.0 - WDBFJK0040HBK-NESN', 130, NULL, 2, NULL),
	(46, 'Brother ADS1500W Compact Color Desktop Scanner with Duplex and Web Connectivity', 180, NULL, 2, NULL),
	(47, 'Brother DS-620 Mobile Color Page Scanner', 120, NULL, 2, NULL),
	(48, 'Mitsubishi Electric Alternator Remanufactured', 52, NULL, 2, NULL),
	(49, 'Mitsubishi Electric Alternator Remanufactured', 46, NULL, 2, NULL),
	(50, '3dRose lsp_207583_1 Northern Cardinal Male in White Pine Tree. Marion. Illinois. USA. Single Toggle Switch', 1, NULL, 6, NULL),
	(51, 'Seagate Backup Plus 8TB Desktop External Hard Drive with 200GB of Cloud Storage & Mobile Device Backup USB 3.0 (STDT8000100)', 230, NULL, 2, NULL),
	(52, 'Sharp - 50 Class (49.7 Diag.) - LED - 1080p - HDTV - Black', 380, NULL, 2, NULL),
	(53, 'DEWALT DWD210G 10-Amp 1/2-Inch Pistol-Grip Drill', 79, NULL, 3, NULL),
	(54, 'DEWALT DWD520K 1/2-Inch VSR Pistol Grip Hammerdrill Kit', 79, NULL, 3, NULL),
	(55, 'DEWALT DWd520 1/2-Inch VSR Pistol Grip Hammerdrill', 79, NULL, 3, NULL),
	(56, 'DeWalt DW511 1/2 (13mm) 7.8 Amp VSR Hammerdrill', 79, NULL, 3, NULL),
	(57, 'InsigniaTM - 32 Class (31-1/2 Diag.) - LED - 1080p - HDTV - Black', 180, NULL, 2, NULL),
	(58, 'ACDelco 252-676 Professional Water Pump', 2, NULL, 6, NULL),
	(59, 'DEWALT DCK280C2 20-Volt Max Li-Ion 1.5 Ah Compact Drill and Impact Driver Combo Kit', 499, NULL, 3, NULL),
	(60, 'Safehouse Signs D-260434 DANGER FLAMMABLE Sign', 3, NULL, 6, NULL),
	(61, 'Canon PowerShot SX530 HS - Wi-Fi Enabled', 280, NULL, 2, NULL),
	(62, 'SodaStream 1/2-Liter Carbonating Bottle. Black. 2-Pack', 16, NULL, 2, NULL),
	(63, 'NCAA Iowa Hawkeyes Satin Etch Pint Glass Set (Pack of 2). 16-Ounce', 3, NULL, 6, NULL),
	(64, 'BISSELL DeepClean Lift-Off Deluxe Pet Full Sized Carpet Cleaner. 24A4', 230, NULL, 2, NULL),
	(65, 'Seagate Backup Plus Slim 1TB Portable External Hard Drive with 200GB of Cloud Storage & Mobile Device Backup USB 3.0 (STDR1000100) - Black', 60, NULL, 2, NULL),
	(66, 'Seagate Backup Plus Slim 2TB Portable External Hard Drive with 200GB of Cloud Storage & Mobile Device Backup USB 3.0 (STDR2000100) - Black', 60, NULL, 2, NULL),
	(67, 'Samsung HT-J4500 5.1 Channel 500 Watt 3D Blu-Ray Home Theater System (2015 Model)', 220, NULL, 2, NULL),
	(68, 'Hitachi 326371 Second Hammer DH40MRY Replacement Part', 1, NULL, 6, NULL),
	(69, 'Hitachi SB8V2 9.0 Amp 3-Inch-by-21-Inch Variable Speed Belt Sander with Trigger Lock and Soft Grip Handles', 129, NULL, 3, NULL),
	(70, 'Logitech G303 Daedalus Apex Performance Edition Gaming Mouse (910-004380)', 25, NULL, 2, NULL),
	(71, 'Samsung HW-J450 2.1 Channel 300 Watt Wireless Audio Soundbar (2015 Model)', 210, NULL, 2, NULL),
	(72, 'Triton 352707 Fixed Handle Inner', 2, NULL, 6, NULL),
	(73, 'Texas Instruments TI-83 Plus Graphing Calculator', 103, NULL, 2, NULL),
	(74, 'Texas Instruments TI-84 Plus Graphics Calculator. Black', 103, NULL, 2, NULL),
	(75, 'Mallory 3760001 Unilite Distributor', 35, NULL, 2, NULL),
	(76, 'TiVo Mini with IR Remote (Old Version)', 119, NULL, 2, NULL),
	(77, 'Herbal Essences Hello Hydration 2-in-1 Moisturizing Hair Shampoo & Conditioner. 10.1 Fl Oz', 3, NULL, 4, NULL),
	(78, 'NETGEAR AC1200 WiFi Range Extender (EX6150-100NAS)', 99, NULL, 2, NULL),
	(79, 'Surya GMN4012-23 Hand Tufted Modern Accent Rug. 2-Feet by 3-Feet', 11, NULL, 6, NULL),
	(12, 'Hamilton Beach 25475A Breakfast Sandwich Maker', 20, NULL, 5, 'asdasda'),
	(80, 'Epson XP-830 Wireless Color Photo Printer with Scanner. Copier & Fax (C11CE78201)', 130, NULL, 2, NULL),
	(81, 'Samsung 2 WAM1500/ZA 1.0 Channel Wireless Speaker (Dark Gray)', 180, NULL, 2, NULL),
	(82, 'LeapFrog LeapPad Platinum Kids Learning Tablet. Green', 102, NULL, 2, NULL),
	(83, 'Canon MG7720 Wireless All-In-One Printer with Scanner and Copier: Mobile and Tablet Printing. with Airprint(TM) and Google Cloud Print compatible. White', 130, NULL, 2, NULL),
	(84, 'Canon MG6821 Wireless All-In-One Printer with Scanner and Copier: Mobile and Tablet Printing with Airprint(TM) and Google Cloud Print compatible', 80, NULL, 2, NULL),
	(85, 'Bresser Spektar 15-45x60 Spotting Scope (Black)', 320, NULL, 2, NULL),
	(86, 'Roku 4 Streaming Media Player (4400R) 4K UHD', 126, NULL, 2, NULL),
	(87, 'Adobe Premiere Elements 14', 70, NULL, 2, NULL),
	(88, 'Adobe Photoshop Elements 14', 70, NULL, 2, NULL),
	(89, 'Pass & Seymour Legrand Canopy Pull Switch Appliance Switches', 18, NULL, 2, NULL),
	(90, 'Parrot Airborne Night MiniDrone - SWAT (Black)', 100, NULL, 2, NULL),
	(91, 'Netgear AC5300 Nighthawk X8 Tri-Band WiFi Router (R8500-100NAS)', 383, NULL, 2, NULL),
	(92, 'Epson WorkForce Pro WF-4630 C11CD10201 Wireless Color All-in-One Inkjet Printer with Scanner and Copier', 200, NULL, 2, NULL),
	(93, 'Epson WorkForce WF-7610 Wireless Color All-in-One Inkjet Printer with Scanner and Copier', 200, NULL, 2, NULL),
	(94, 'Epson WorkForce WF-7620 Wireless Color All-in-One Inkjet Printer with Scanner and Copier', 200, NULL, 2, NULL),
	(95, 'Epson WorkForce WF-3620 WiFi Direct All-in-One Color Inkjet Printer. Copier. Scanner', 90, NULL, 2, NULL),
	(96, 'Canon Selphy CP1200 Black Wireless Color Photo Printer', 100, NULL, 2, NULL),
	(97, 'Canon Selphy CP1200 Black Wireless Color Photo Printer and Battery Bundle', 100, NULL, 2, NULL),
	(98, 'Tag Seagrass Basket. Large 25-1/4-Inch Long Rectangular Shallow Basket', 6, NULL, 4, NULL),
	(99, 'Epson Expression Home XP-430 Wireless Color Photo Printer with Scanner and Copier', 60, NULL, 2, NULL),
	(100, 'Speck Samsung Galaxy S 7 Candyshell Inked Case', 39, NULL, 2, NULL),
	(101, 'Samsung Galaxy S 7 Candyshell Inked Case', 39, NULL, 2, NULL),
	(102, 'Fire Tablet. 7 Display. Wi-Fi. 16 GB - Includes Special Offers. Black', 60, NULL, 2, NULL),
	(103, 'Corsair Gaming M65 PRO RGB FPS Gaming Mouse. Backlit RGB LED. 12000 DPI. Optical', 50, NULL, 2, NULL),
	(104, 'Sunpak Platinum Plus by Sunpak 4200XL Tabletop Tripod (Black)', 16, NULL, 2, NULL),
	(105, 'NETGEAR AC1600 Dual Band Wi-Fi Gigabit Router (R6250)', 235, NULL, 2, NULL),
	(106, 'MiP Robot (White)', 67, NULL, 2, NULL),
	(107, 'WD 2TB Black My Passport Ultra Portable External Hard Drive - USB 3.0 - WDBBKD0020BBK-NESN', 100, NULL, 2, NULL),
	(108, 'Belkin NetCam HD+ Wi-Fi enabled Camera works with WeMo. includes Night Vision. All Glass Wide Angle Lens. and Infrared Cut-off Filter', 35, NULL, 2, NULL),
	(109, 'Brother HL-L2340DW Compact Laser Printer. Monochrome. Wireless. Duplex Printing. Amazon Dash Replenishment Enabled', 100, NULL, 2, NULL),
	(110, 'Le Creuset Enamel-on-Steel 8-Quart Covered Stockpot. Marseille', 95, NULL, 4, NULL),
	(111, 'Le Creuset Enamel-on-Steel 8-Quart Covered Stockpot. Caribbean', 95, NULL, 4, NULL),
	(112, 'Farberware Aluminum Nonstick 8-Inch. 10-Inch and 11-Inch Dishwasher Safe Skillet Triple Pack', 40, NULL, 4, NULL),
	(113, 'Actiontec My Wireless TV WiFi / HDMI Multi-Room Wireless HD Video Kit - 2nd Generation (MWTV2KIT01)', 170, NULL, 2, NULL),
	(114, 'Lowepro Photo Hatchback 16L Camera Backpack - Daypack Style Backpack For DSLR and Mirrorless Cameras', 73, NULL, 2, NULL),
	(115, 'Frigidaire FAD504DWD Energy Star 50-pint Dehumidifier', 199, NULL, 3, NULL),
	(116, 'Logitech Keys-To-Go Ultra-Portable Bluetooth Keyboard for Android and Windows. Dark Blue (920-007196)', 50, NULL, 2, NULL),
	(117, 'NATIONAL NAIL 0616190 4K 3-1/4-Inch Smooth Frame Nail', 33, NULL, 3, NULL),
	(118, 'Altec Lansing The Jacket Bluetooth Speaker. Black (iMW455)', 130, NULL, 2, NULL),
	(119, 'Design House 702019 Commercial Grade C-Series Passage 2-Way Curved Lever. Satin Chrome Finish', 1, NULL, 6, NULL),
	(120, 'Shark NV501 Rotator Professional Lift-Away Vacuum Cleaner', 220, NULL, 2, NULL),
	(121, 'Halo: The Master Chief Collection', 20, NULL, 2, NULL),
	(122, 'Sony BDPS6500 3D 4K Upscaling Blu-ray Player with Wi-Fi (2015 Model)', 110, NULL, 2, NULL),
	(123, 'ZAGG InvisibleShield HD Glass for Samsung Galaxy Tab A 9.7 Screen (T97HGS-F00)', 15, NULL, 2, NULL),
	(124, 'APC BE450G Back-UPS ES 8-Outlet 450VA 120V Uninterrupted Power Supply (Discontinued by Manufacturer)', 54, NULL, 2, NULL),
	(125, 'Tom Clancy Rainbow Six Siege - Xbox One', 30, NULL, 2, NULL),
	(126, 'Garmin 10-24V - 2amp Vehicle Power Cable', 17, NULL, 2, NULL),
	(127, 'THINKWARE H50 HD Dash Cam with 2.0MP CMOS Camera', 60, NULL, 2, NULL),
	(128, 'Shark Navigator Lift-Away Deluxe (NV360)', 164, NULL, 2, NULL),
	(129, 'SKIL 5180-01 14-Amp. 7-1/4-Inch Circular Saw', 65, NULL, 3, NULL),
	(130, 'Canon PIXMA MX922 Wireless Office All-In-One Printer', 100, NULL, 2, NULL),
	(131, 'Shark Rocket DeluxePro Upright Vacuum HV321', 190, NULL, 2, NULL),
	(132, 'Nespresso Inissia Espresso Maker. Black', 118, NULL, 2, NULL),
	(133, 'Epson WorkForce WF-2650 All-In-One Wireless Color Printer with Scanner. Copier and Fax', 70, NULL, 2, NULL),
	(134, 'SiriusXM SXV300v1 Connect Vehicle Tuner Kit for Satellite Radio', 45, NULL, 2, NULL),
	(135, 'Sony LCJRXF/B Premium Jacket Case (Black)', 57, NULL, 2, NULL),
	(136, 'Incase Dual Kit GoPro Case', 51, NULL, 2, NULL),
	(137, 'Thermaltake BlacX Duet eSATA USB Dual Hard Drives Docking Station ST0014U', 39, NULL, 2, NULL),
	(138, 'ASUS (RT-AC68U) Wireless-AC1900 Dual-Band Gigabit Router', 180, NULL, 2, NULL),
	(139, 'Brother MFC9130CW Wireless All-In-One Printer with Scanner. Copier and Fax. Amazon Dash Replenishment Enabled', 280, NULL, 2, NULL),
	(140, 'Tomtom VIA 1515M Automobile Portable GPS Navigator - Black. Gray', 100, NULL, 2, NULL),
	(141, 'Brother HL-3170CDW Digital Color Printer with Wireless Networking and Duplex', 180, NULL, 2, NULL),
	(142, 'RCA RCRN06GR 4 Device Universal Remote', 17, NULL, 2, NULL),
	(143, 'Samsung Galaxy Note 4 Case. S View Flip Cover Folio Case - White', 22, NULL, 2, NULL),
	(144, 'Solo Urban 15.6 Laptop Slim Brief. Black. UBN101-4', 25, NULL, 2, NULL),
	(145, 'Philips Sonicare DiamondClean Sonic Electric Rechargeable Toothbrush. Pink. HX9362/68', 207, NULL, 2, NULL),
	(146, 'C.R. Gibson Small Leather Bound Journal. UCLA Bruins (C920705WM)', 3, NULL, 6, NULL),
	(147, 'NETGEAR Nighthawk AC1900 Dual Band Wi-Fi Gigabit Router (R7000)', 230, NULL, 2, NULL),
	(148, 'Garmin Friction Mount', 18, NULL, 2, NULL),
	(149, 'Linksys Dual-Band AC1200 Wireless USB 3.0 Adapter (WUSB6300)', 58, NULL, 2, NULL),
	(150, 'Halo 5: Guardians', 40, NULL, 2, NULL),
	(151, 'Belkin USB-IF Certified 3-Foot 3.1 USB Type C (USB-C) to USB Type C Cable', 25, NULL, 2, NULL),
	(152, 'Canon MAXIFY MB2320 Wireless Office All-In-One Inkjet Printer with Mobile and Tablet Printing. Black', 90, NULL, 2, NULL),
	(153, 'Logitech Professional Presenter R800 with Green Laser Pointer', 62, NULL, 2, NULL),
	(154, 'PNY Attaché 8GB USB 2.0 Flash Drive - P-FD8GBATT03-GE', 6, NULL, 2, NULL),
	(155, 'Knipex Tools 98 37 16 Socket', 2, NULL, 6, NULL),
	(156, 'Daisy Outdoor Products Elite (Brown/Black. 39.75 Inch)', 5, NULL, 6, NULL);


--
-- Data for Name: Locations; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Locations" ("LocationID", "LocationOwnerID", "Adress", "CityID", "LastModifier") OVERRIDING SYSTEM VALUE VALUES
	(14, '18749dc4-0c10-4a42-bc44-23f5b9c512b9', 'Kemalpaşa Esentepe Kampüsü, Üniversite Cd. (Bilişim Sistemleri Fakültesi)', 175, 'Hakki');


--
-- Data for Name: Operations; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Operations" ("OperationID", "OperationValue", "Date", "BasketID", "LocationID", "VehicleID", "OwnerID", "LastModifier") OVERRIDING SYSTEM VALUE VALUES
	(19, 2242, '2021-12-19 14:14:55.703549+03', 11, 14, 6, '18749dc4-0c10-4a42-bc44-23f5b9c512b9', 'Hakki');


--
-- Data for Name: Vehicles; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public."Vehicles" ("VehicleID", "VehicleName", "VehiclePlate", "LastModifier") OVERRIDING SYSTEM VALUE VALUES
	(5, 'Ford 2642 HR', '34 HR 0654', 'Hakki'),
	(6, 'Ford 1848T Midilli', '34 TM 1556', 'Hakki'),
	(7, 'Ford 1842', '54 FT 1856', 'Hakki');


--
-- Name: Actions_ActionID_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Actions_ActionID_seq"', 38, true);


--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."AspNetRoleClaims_Id_seq"', 1, false);


--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."AspNetUserClaims_Id_seq"', 1, false);


--
-- Name: Items_ItemID_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Items_ItemID_seq"', 156, true);


--
-- Name: Locations_LocationID_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Locations_LocationID_seq"', 15, true);


--
-- Name: Operations_OperationID_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Operations_OperationID_seq"', 19, true);


--
-- Name: Vehicles_VehicleID_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Vehicles_VehicleID_seq"', 7, true);


--
-- Name: alerts_alertid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.alerts_alertid_seq', 30, true);


--
-- Name: basket_basketid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.basket_basketid_seq', 11, true);


--
-- Name: basket_items_basketitemid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.basket_items_basketitemid_seq', 79, true);


--
-- Name: category_categoryid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.category_categoryid_seq', 7, true);


--
-- Name: city_cityid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.city_cityid_seq', 40, true);


--
-- Name: country_countryid_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.country_countryid_seq', 23, true);


--
-- Name: Alerts Alerts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Alerts"
    ADD CONSTRAINT "Alerts_pkey" PRIMARY KEY ("AlertID");


--
-- Name: Basket_items Basket_items_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Basket_items"
    ADD CONSTRAINT "Basket_items_pkey" PRIMARY KEY ("ItemID", "BasketID");


--
-- Name: Actions PK_Actions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Actions"
    ADD CONSTRAINT "PK_Actions" PRIMARY KEY ("ActionID");


--
-- Name: AspNetRoleClaims PK_AspNetRoleClaims; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");


--
-- Name: AspNetRoles PK_AspNetRoles; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetRoles"
    ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");


--
-- Name: AspNetUserClaims PK_AspNetUserClaims; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");


--
-- Name: AspNetUserLogins PK_AspNetUserLogins; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");


--
-- Name: AspNetUserRoles PK_AspNetUserRoles; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId");


--
-- Name: AspNetUserTokens PK_AspNetUserTokens; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");


--
-- Name: AspNetUsers PK_AspNetUsers; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUsers"
    ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");


--
-- Name: Basket PK_Basket; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Basket"
    ADD CONSTRAINT "PK_Basket" PRIMARY KEY ("BasketID");


--
-- Name: Category PK_Category; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Category"
    ADD CONSTRAINT "PK_Category" PRIMARY KEY ("CategoryID");


--
-- Name: Country PK_Country; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Country"
    ADD CONSTRAINT "PK_Country" PRIMARY KEY ("CountryID");


--
-- Name: Items PK_Items; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Items"
    ADD CONSTRAINT "PK_Items" PRIMARY KEY ("ItemID");


--
-- Name: Locations PK_Locations; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Locations"
    ADD CONSTRAINT "PK_Locations" PRIMARY KEY ("LocationID");


--
-- Name: Operations PK_Operations; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Operations"
    ADD CONSTRAINT "PK_Operations" PRIMARY KEY ("OperationID");


--
-- Name: City PK_State; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."City"
    ADD CONSTRAINT "PK_State" PRIMARY KEY ("CityID");


--
-- Name: Vehicles PK_Vehicles; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Vehicles"
    ADD CONSTRAINT "PK_Vehicles" PRIMARY KEY ("VehicleID");


--
-- Name: EmailIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "EmailIndex" ON public."AspNetUsers" USING btree ("NormalizedEmail");


--
-- Name: IX_AspNetRoleClaims_RoleId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON public."AspNetRoleClaims" USING btree ("RoleId");


--
-- Name: IX_AspNetUserClaims_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AspNetUserClaims_UserId" ON public."AspNetUserClaims" USING btree ("UserId");


--
-- Name: IX_AspNetUserLogins_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AspNetUserLogins_UserId" ON public."AspNetUserLogins" USING btree ("UserId");


--
-- Name: IX_AspNetUserRoles_RoleId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON public."AspNetUserRoles" USING btree ("RoleId");


--
-- Name: IX_Items_CategoryID; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Items_CategoryID" ON public."Items" USING btree ("CategoryID");


--
-- Name: IX_Locations_LocationOwnerID; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Locations_LocationOwnerID" ON public."Locations" USING btree ("LocationOwnerID");


--
-- Name: IX_Locations_StateID; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Locations_StateID" ON public."Locations" USING btree ("CityID");


--
-- Name: RoleNameIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "RoleNameIndex" ON public."AspNetRoles" USING btree ("NormalizedName");


--
-- Name: UserNameIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "UserNameIndex" ON public."AspNetUsers" USING btree ("NormalizedUserName");


--
-- Name: index_BasketID; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "index_BasketID" ON public."Basket_items" USING btree ("BasketID");


--
-- Name: index_BasketItemID; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "index_BasketItemID" ON public."Basket_items" USING btree ("BasketItemID");


--
-- Name: index_EffecterUser; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "index_EffecterUser" ON public."Actions" USING btree ("EffecterUser");


--
-- Name: Items logger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER logger AFTER INSERT OR UPDATE ON public."Items" FOR EACH ROW EXECUTE FUNCTION public."logItemChange"();


--
-- Name: Locations logger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER logger AFTER INSERT OR UPDATE ON public."Locations" FOR EACH ROW EXECUTE FUNCTION public."logLocationChange"();


--
-- Name: Operations logger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER logger AFTER UPDATE ON public."Operations" FOR EACH ROW EXECUTE FUNCTION public."logOperationStart"();


--
-- Name: Vehicles logger; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER logger AFTER INSERT OR UPDATE ON public."Vehicles" FOR EACH ROW EXECUTE FUNCTION public."logVehicleChange"();


--
-- Name: AspNetRoleClaims FK_AspNetRoleClaims_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserClaims FK_AspNetUserClaims_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserLogins FK_AspNetUserLogins_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserTokens FK_AspNetUserTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: Basket_items basket_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Basket_items"
    ADD CONSTRAINT basket_id FOREIGN KEY ("BasketID") REFERENCES public."Basket"("BasketID") ON DELETE RESTRICT;


--
-- Name: Items category; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Items"
    ADD CONSTRAINT category FOREIGN KEY ("CategoryID") REFERENCES public."Category"("CategoryID") ON DELETE RESTRICT;


--
-- Name: City country_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."City"
    ADD CONSTRAINT country_id FOREIGN KEY ("CountryID") REFERENCES public."Country"("CountryID") NOT VALID;


--
-- Name: Basket_items item_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Basket_items"
    ADD CONSTRAINT item_id FOREIGN KEY ("ItemID") REFERENCES public."Items"("ItemID") ON DELETE RESTRICT;


--
-- Name: Locations owner_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Locations"
    ADD CONSTRAINT owner_id FOREIGN KEY ("LocationOwnerID") REFERENCES public."AspNetUsers"("Id") NOT VALID;


--
-- Name: Operations owner_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Operations"
    ADD CONSTRAINT owner_id FOREIGN KEY ("OwnerID") REFERENCES public."AspNetUsers"("Id") NOT VALID;


--
-- Name: Locations state_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Locations"
    ADD CONSTRAINT state_id FOREIGN KEY ("CityID") REFERENCES public."City"("CityID") ON DELETE RESTRICT;


--
-- Name: Basket user_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Basket"
    ADD CONSTRAINT user_id FOREIGN KEY ("UserID") REFERENCES public."AspNetUsers"("Id") NOT VALID;


--
-- Name: Alerts user_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Alerts"
    ADD CONSTRAINT user_id FOREIGN KEY ("UserID") REFERENCES public."AspNetUsers"("Id") NOT VALID;


--
-- Name: Operations vehicle_id; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Operations"
    ADD CONSTRAINT vehicle_id FOREIGN KEY ("VehicleID") REFERENCES public."Vehicles"("VehicleID") NOT VALID;


--
-- PostgreSQL database dump complete
--


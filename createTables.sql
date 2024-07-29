CREATE TABLE public."Products" (
	id uuid NOT NULL,
	description text DEFAULT "No Description" NULL,
	imagefile text DEFAULT "noImage.png" NULL,
	"name" text NOT NULL,
	price decimal NOT NULL,
	CONSTRAINT products_pk PRIMARY KEY (id)
);
CREATE TABLE public."Categories" (
	id uuid NOT NULL,
	"name" text NOT NULL,
	CONSTRAINT "_Categories__pk" PRIMARY KEY (id),
	CONSTRAINT "_Categories__unique" UNIQUE ("name")
);
CREATE TABLE public."Products_Categories" (
	product_id uuid NOT NULL,
	category_id uuid NOT NULL,
	CONSTRAINT "Products_Categories_pkey" PRIMARY KEY (product_id, category_id)
);
ALTER TABLE public."Products_Categories" ADD CONSTRAINT "Products_Categories_category_id_fkey" FOREIGN KEY (category_id) REFERENCES public."Categories"(id);
ALTER TABLE public."Products_Categories" ADD CONSTRAINT "Products_Categories_product_id_fkey" FOREIGN KEY (product_id) REFERENCES public."Products"(id);


CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
INSERT INTO public."Categories" (id, "name")
VALUES
(uuid_generate_v4(), 'Technology'),
(uuid_generate_v4(), 'Home'),
(uuid_generate_v4(), 'VideoGames'),
(uuid_generate_v4(), 'Clean'),
(uuid_generate_v4(), 'Home appliances');
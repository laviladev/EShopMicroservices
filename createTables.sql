CREATE TABLE public."products" (
	id uuid NOT NULL,
	description text NULL,
	imagefile text NULL,
	"name" text NOT NULL,
	price decimal NOT NULL,
	CONSTRAINT products_pk PRIMARY KEY (id)
);
CREATE TABLE public."categories" (
	id uuid NOT NULL,
	"name" text NOT NULL,
	CONSTRAINT "_categories__pk" PRIMARY KEY (id),
	CONSTRAINT "_categories__unique" UNIQUE ("name")
);
CREATE TABLE public."products_categories" (
	product_id uuid NOT NULL,
	category_id uuid NOT NULL,
	CONSTRAINT "products_categories_pkey" PRIMARY KEY (product_id, category_id)
);
ALTER TABLE public."products_categories" ADD CONSTRAINT "products_categories_category_id_fkey" FOREIGN KEY (category_id) REFERENCES public."categories"(id);
ALTER TABLE public."products_categories" ADD CONSTRAINT "products_categories_product_id_fkey" FOREIGN KEY (product_id) REFERENCES public."products"(id);


CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
INSERT INTO public."categories" (id, "name")
VALUES
(uuid_generate_v4(), 'Technology'),
(uuid_generate_v4(), 'Home'),
(uuid_generate_v4(), 'VideoGames'),
(uuid_generate_v4(), 'Clean'),
(uuid_generate_v4(), 'Home appliances');
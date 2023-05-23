
DROP TABLE IF EXISTS public.mt_doc_driver CASCADE;
CREATE TABLE public.mt_doc_driver (
                                      id                  uuid                        NOT NULL,
                                      data                jsonb                       NOT NULL,
                                      mt_last_modified    timestamp with time zone    NULL DEFAULT (transaction_timestamp()),
                                      mt_version          uuid                        NOT NULL DEFAULT (md5(random()::text || clock_timestamp()::text)::uuid),
                                      mt_dotnet_type      varchar                     NULL,
                                      CONSTRAINT pkey_mt_doc_driver_id PRIMARY KEY (id)
);

CREATE INDEX mt_doc_driver_idx_license_type 
    ON public.mt_doc_driver USING btree ((data ->> 'LicenseType'));

CREATE OR REPLACE FUNCTION public.mt_upsert_driver(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
final_version uuid;
BEGIN
INSERT INTO public.mt_doc_driver ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp())
    ON CONFLICT ON CONSTRAINT pkey_mt_doc_driver_id
    DO UPDATE SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp();

SELECT mt_version FROM public.mt_doc_driver into final_version WHERE id = docId ;
RETURN final_version;
END;
$function$;


CREATE OR REPLACE FUNCTION public.mt_insert_driver(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
BEGIN
INSERT INTO public.mt_doc_driver ("data", "mt_dotnet_type", "id", "mt_version", mt_last_modified) VALUES (doc, docDotNetType, docId, docVersion, transaction_timestamp());

RETURN docVersion;
END;
$function$;


CREATE OR REPLACE FUNCTION public.mt_update_driver(doc JSONB, docDotNetType varchar, docId uuid, docVersion uuid) RETURNS UUID LANGUAGE plpgsql SECURITY INVOKER AS $function$
DECLARE
final_version uuid;
BEGIN
UPDATE public.mt_doc_driver SET "data" = doc, "mt_dotnet_type" = docDotNetType, "mt_version" = docVersion, mt_last_modified = transaction_timestamp() where id = docId;

SELECT mt_version FROM public.mt_doc_driver into final_version WHERE id = docId ;
RETURN final_version;
END;
$function$;



select public.mt_upsert_driver(:p0, :p1, :p2, :p3);
p0: {"Id":"0186eb27-ad16-4b7b-a46f-34bd526ff877","FirstName":"Patrick","LastName":"Mahomes","LicenseType":"Commercial"}
  p1: RideSharing.Driver
  p2: 0186eb27-ad16-4b7b-a46f-34bd526ff877
  p3: 0186eb27-ad17-4cf4-892b-4e2e71740040
Persisted 1 updates, 0 inserts, and 0 deletions
select d.id, d.data from public.mt_doc_driver as d where id = :id
    id: 0186eb27-ad16-4b7b-a46f-34bd526ff877

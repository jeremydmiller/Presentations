DROP TABLE IF EXISTS public.mt_streams CASCADE;
CREATE TABLE public.mt_streams (
                                   id                  uuid           NOT NULL,
                                   type                varchar        NULL,
                                   version             bigint         NULL,
                                   timestamp           timestamptz    NOT NULL DEFAULT (now()),
                                   snapshot            jsonb          NULL,
                                   snapshot_version    integer        NULL,
                                   created             timestamptz    NOT NULL DEFAULT (now()),
                                   tenant_id           varchar        NULL DEFAULT '*DEFAULT*',
                                   is_archived         bool           NULL DEFAULT FALSE,
                                   CONSTRAINT pkey_mt_streams_id PRIMARY KEY (id)
);
DROP TABLE IF EXISTS public.mt_events CASCADE;
CREATE TABLE public.mt_events (
                                  seq_id            bigint                      NOT NULL,
                                  id                uuid                        NOT NULL,
                                  stream_id         uuid                        NULL,
                                  version           bigint                      NOT NULL,
                                  data              jsonb                       NOT NULL,
                                  type              varchar(500)                NOT NULL,
                                  timestamp         timestamp with time zone    NOT NULL DEFAULT '(now())',
                                  tenant_id         varchar                     NULL DEFAULT '*DEFAULT*',
                                  mt_dotnet_type    varchar                     NULL,
                                  is_archived       bool                        NULL DEFAULT FALSE,
                                  CONSTRAINT pkey_mt_events_seq_id PRIMARY KEY (seq_id)
);

ALTER TABLE public.mt_events
    ADD CONSTRAINT fkey_mt_events_stream_id FOREIGN KEY(stream_id)
        REFERENCES public.mt_streams(id)ON DELETE CASCADE
;
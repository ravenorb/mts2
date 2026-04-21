-- =========================================================
-- ITEM MASTER
-- =========================================================

create table item (
    id integer primary key autoincrement,
    item_no text not null unique,
    item_type text not null,                -- frame_part, frame_assembly, cut_sheet, component_part, purchased_part
    title text not null,
    description text,
    unit_of_measure text not null default 'ea',
    lifecycle_state text not null default 'draft',
    created_at text not null default (datetime('now')),
    updated_at text not null default (datetime('now'))
);

create table item_revision (
    id integer primary key autoincrement,
    item_id integer not null,
    revision_code text not null,
    revision_note text,
    is_current integer not null default 0,
    release_state text not null default 'draft',   -- draft, released, superseded
    effective_from text,
    effective_to text,
    created_by text,
    created_at text not null default (datetime('now')),
    foreign key (item_id) references item(id) on delete cascade,
    unique (item_id, revision_code)
);

create index ix_item_revision_item_id on item_revision(item_id);
create index ix_item_revision_item_current on item_revision(item_id, is_current);

-- =========================================================
-- ITEM SUBTYPES
-- =========================================================

create table frame_part (
    item_id integer primary key,
    family_code text,
    material_spec text,
    default_finish text,
    foreign key (item_id) references item(id) on delete cascade
);

create table frame_assembly (
    item_id integer primary key,
    family_code text,
    assembly_class text,
    foreign key (item_id) references item(id) on delete cascade
);

create table cut_sheet (
    item_id integer primary key,
    machine_type text,                      -- laser, waterjet
    material_spec text,
    gauge text,
    sheet_size text,
    foreign key (item_id) references item(id) on delete cascade
);

create table component_part (
    item_id integer primary key,
    make_buy text not null default 'make', -- make, buy
    vendor_part_no text,
    foreign key (item_id) references item(id) on delete cascade
);

-- =========================================================
-- DOCUMENTS
-- =========================================================

create table document (
    id integer primary key autoincrement,
    document_no text not null unique,
    document_type text not null,            -- drawing, cutsheet_pdf, bend_sheet, traveler, work_instruction, machine_file
    title text not null,
    lifecycle_state text not null default 'draft',
    created_at text not null default (datetime('now'))
);

create table document_revision (
    id integer primary key autoincrement,
    document_id integer not null,
    revision_code text not null,
    file_name text not null,
    file_path text not null,
    mime_type text,
    checksum_sha256 text,
    file_size_bytes integer,
    is_current integer not null default 0,
    release_state text not null default 'draft',
    created_by text,
    created_at text not null default (datetime('now')),
    foreign key (document_id) references document(id) on delete cascade,
    unique (document_id, revision_code)
);

create index ix_document_revision_document_id on document_revision(document_id);
create index ix_document_revision_document_current on document_revision(document_id, is_current);

-- =========================================================
-- REVISION <-> DOCUMENT LINKS
-- =========================================================

create table revision_document_link (
    id integer primary key autoincrement,
    item_revision_id integer not null,
    document_revision_id integer not null,
    document_role text not null,            -- frame_part_drawing, frame_assembly_drawing, cut_sheet_pdf, cut_sheet_drawing, bend_sheet, traveler, work_instruction
    is_primary integer not null default 0,
    sort_order integer not null default 0,
    foreign key (item_revision_id) references item_revision(id) on delete cascade,
    foreign key (document_revision_id) references document_revision(id) on delete cascade,
    unique (item_revision_id, document_revision_id, document_role)
);

create index ix_revision_document_link_item_revision_id on revision_document_link(item_revision_id);
create index ix_revision_document_link_document_revision_id on revision_document_link(document_revision_id);

-- =========================================================
-- FRAME PART REVISION <-> CUT SHEET REVISION LINKS
-- =========================================================

create table frame_part_cut_sheet_link (
    id integer primary key autoincrement,
    frame_part_revision_id integer not null,
    cut_sheet_revision_id integer not null,
    usage_qty numeric,
    link_type text not null default 'manufactured_from',   -- manufactured_from, alternate_source, nests_on
    is_primary integer not null default 1,
    notes text,
    foreign key (frame_part_revision_id) references item_revision(id) on delete cascade,
    foreign key (cut_sheet_revision_id) references item_revision(id) on delete cascade,
    unique (frame_part_revision_id, cut_sheet_revision_id, link_type)
);

create index ix_fp_cs_link_frame_part_revision_id on frame_part_cut_sheet_link(frame_part_revision_id);
create index ix_fp_cs_link_cut_sheet_revision_id on frame_part_cut_sheet_link(cut_sheet_revision_id);

-- =========================================================
-- BOM
-- =========================================================

create table item_bom (
    id integer primary key autoincrement,
    parent_revision_id integer not null,
    child_revision_id integer not null,
    qty numeric not null,
    find_no text,
    bom_role text not null default 'component',   -- component, subassembly, hardware, consumable
    sort_order integer not null default 0,
    notes text,
    foreign key (parent_revision_id) references item_revision(id) on delete cascade,
    foreign key (child_revision_id) references item_revision(id) on delete restrict
);

create index ix_item_bom_parent_revision_id on item_bom(parent_revision_id);
create index ix_item_bom_child_revision_id on item_bom(child_revision_id);
create unique index ux_item_bom_parent_child_findno on item_bom(parent_revision_id, child_revision_id, ifnull(find_no, ''));

-- =========================================================
-- OPTIONAL: CUT SHEET BOM LINES
-- Useful if parsed cutsheet rows need preserving separately
-- =========================================================

create table cut_sheet_bom_line (
    id integer primary key autoincrement,
    cut_sheet_revision_id integer not null,
    component_item_id integer,
    line_no integer not null default 0,
    part_no text,
    description text,
    qty numeric not null default 1,
    notes text,
    foreign key (cut_sheet_revision_id) references item_revision(id) on delete cascade,
    foreign key (component_item_id) references item(id) on delete set null
);

create index ix_cut_sheet_bom_line_cut_sheet_revision_id on cut_sheet_bom_line(cut_sheet_revision_id);
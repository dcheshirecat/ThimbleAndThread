# Art list: every screen and picture, with its file name

Everything in the game is a placeholder for now (coloured shapes, or flat drawings made by code). This is the full
list of what to draw, each with the **exact file name** to give it. Tick things off as you go.

- **How to hand art over:** see [ART_SWAP.md](ART_SWAP.md) (the Google Drive folder "Game Art - Thimble and Thread").
- **Colour profile sRGB, sizes, transparency:** [ART_SIZES.md](ART_SIZES.md). Procreate tips: [ADDING_ART.md](ADDING_ART.md).
- **File names:** all lowercase, underscores instead of spaces, `.png`. Name the file exactly as listed here. The folder
  in the game is shown in each section; in the Drive folder any subfolder is fine.
- **"New"** in a table means the game draws a shape there today and has no file yet: make the PNG with that name and
  Claude wires it in. Everything else replaces a placeholder file of the same name.
- Size: draw at that size or bigger, with the **same shape** (aspect ratio).
- The repo is public for now, so anything committed to it can be seen and copied.

## Part 1: UI

### 1a. Screen mock-ups (layouts, for Claude to build from; not used in the game as they are)
Draw each screen as you'd like it to look, 2048 x 2732 (or 2048 x 4440 to include the extra top and bottom of tall
phones). Put them in a **Mockups** subfolder. From these, Claude rebuilds the screen and lists the pieces to cut out (most
of them are already named in 1b-1e below).
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `mockup_title.png` | Title screen (logo, Play, How to Play) | 2048 x 2732 |
| [ ] | `mockup_save_select.png` | Save select: 3 slots, New / Continue / Delete, the confirm pop-up | 2048 x 2732 |
| [ ] | `mockup_how_to_play.png` | How to Play / tutorial page (picture + text) | 2048 x 2732 |
| [ ] | `mockup_customize_look.png` | Customize: Your Look (name, pronouns, skin, hair, top, Your Makes) | 2048 x 2732 |
| [ ] | `mockup_customize_shop.png` | Customize: Your Shop tab | 2048 x 2732 |
| [ ] | `mockup_shop_counter.png` | Shop by day, middle page: you behind the counter, the door, decor spots | 2048 x 2732 |
| [ ] | `mockup_shop_pickups.png` | Shop, left page: the pickup corner | 2048 x 2732 |
| [ ] | `mockup_shop_display.png` | Shop, right page: the pattern display | 2048 x 2732 |
| [ ] | `mockup_shops_menu.png` | Shops menu under the coin counter (hours, shut shops greyed) | 2048 x 2732 |
| [ ] | `mockup_decorating.png` | Decorating: the pencil tray and glowing spots | 2048 x 2732 |
| [ ] | `mockup_online_orders.png` | Online Orders panel (the computer on the counter) | 2048 x 2732 |
| [ ] | `mockup_dialogue.png` | Talking to a customer: portrait, nameplate, line, choices | 2048 x 2732 |
| [ ] | `mockup_day_summary.png` | End-of-day summary | 2048 x 2732 |
| [ ] | `mockup_workroom.png` | Workroom by night | 2048 x 2732 |
| [ ] | `mockup_order_book.png` | Order book | 2048 x 2732 |
| [ ] | `mockup_workbench.png` | Workbench: an order's steps, tools and materials | 2048 x 2732 |
| [ ] | `mockup_gather.png` | Gather: cupboard tiles and the needs list | 2048 x 2732 |
| [ ] | `mockup_crafting.png` | Crafting: the work, hint, colours, Undo / Done for Now / Finish, zoom buttons | 2048 x 2732 |
| [ ] | `mockup_result.png` | Result: quality, special touches, Set Aside | 2048 x 2732 |
| [ ] | `mockup_pattern_studio.png` | Pattern Studio list and pattern page | 2048 x 2732 |
| [ ] | `mockup_pattern_designer.png` | Pattern designer: grid, colours, stitch type, cloth, naming pop-up | 2048 x 2732 |
| [ ] | `mockup_supply_market.png` | The Supply Market: shelves of tiles, shopping list, special order | 2048 x 2732 |
| [ ] | `mockup_boutique.png` | Moreau's Boutique: rail and mirror | 2048 x 2732 |
| [ ] | `mockup_pattern_shop.png` | Finch's Pattern Parlour: patterns and magazines | 2048 x 2732 |
| [ ] | `mockup_workshop.png` | Marsh's Workshop: furniture, attic and basement | 2048 x 2732 |
| [ ] | `mockup_curios.png` | Quill's Curiosities | 2048 x 2732 |
| [ ] | `mockup_tech_shop.png` | Byte & Bobbin | 2048 x 2732 |
| [ ] | `mockup_town_map.png` | Saturday town map with pins and the place card | 2048 x 2732 |
| [ ] | `mockup_stall.png` | Sunday market stall (shopper card, goals, Sell / Ask for More / Keep It) | 2048 x 2732 |
| [ ] | `mockup_journal.png` | Journal: the open book with tabs | 2048 x 2732 |
| [ ] | `mockup_journal_friends.png` | Journal: a friend card (portrait, meter, notes, likes) | 2048 x 2732 |
| [ ] | `mockup_journal_stamps.png` | Journal: the stamp album | 2048 x 2732 |
| [ ] | `mockup_skills.png` | Skills report (from the XP bar) | 2048 x 2732 |
| [ ] | `mockup_notices.png` | Notices list ("What's new") | 2048 x 2732 |
| [ ] | `mockup_calendar.png` | Calendar: 4 seasons of 28 days, birthdays | 2048 x 2732 |
| [ ] | `mockup_settings.png` | Settings and the pause menu | 2048 x 2732 |
| [ ] | `mockup_trade.png` | Trade: Give / Sell / Surprise / Patterns / Open / My Codes | 2048 x 2732 |
| [ ] | `mockup_popup.png` | A pop-up card (explainers, Goal reached, doorbell notice) | 2048 x 2732 |

### 1b. Shared UI pieces (used on every screen; folder `Art/UI/`)
Panels and buttons stretch, so draw them as **9-slice**: decorated corners and edges, plain middle, the same border
width all round. Tell Claude the border width (e.g. "48 px").
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `ui_panel.png` | Panel / card frame (9-slice) | 512 x 512 |
| [ ] | `ui_panel_popup.png` | Pop-up card (explainers, Goal reached, notices) (9-slice) | 512 x 512 |
| [ ] | `ui_panel_dialogue.png` | Dialogue box (9-slice) | 512 x 512 |
| [ ] | `ui_nameplate.png` | Nameplate on the dialogue box (9-slice) | 512 x 160 |
| [ ] | `ui_button_normal.png` | Button (9-slice) | 512 x 192 |
| [ ] | `ui_button_pressed.png` | Button, pressed | 512 x 192 |
| [ ] | `ui_button_disabled.png` | Button, greyed out | 512 x 192 |
| [ ] | `ui_button_round.png` | Small round button (pencil, map pins, close) | 256 x 256 |
| [ ] | `ui_tab.png` | Tab (Journal, Customize, Trade) | 384 x 160 |
| [ ] | `ui_tab_selected.png` | Tab, the open one | 384 x 160 |
| [ ] | `ui_list_row.png` | List row (orders, shop lists) (9-slice) | 512 x 192 |
| [ ] | `ui_tile.png` | Shop tile (market, boutique, workshop) (9-slice) | 512 x 512 |
| [ ] | `ui_tile_rare.png` | Shop tile, rare (gold) | 512 x 512 |
| [ ] | `ui_bar_back.png` | Progress bar, empty (XP, friendship) (9-slice) | 512 x 64 |
| [ ] | `ui_bar_fill.png` | Progress bar, fill (9-slice) | 512 x 64 |
| [ ] | `ui_badge.png` | Round count badge (notices bell) | 128 x 128 |
| [ ] | `ui_swatch_frame.png` | Frame round a colour swatch | 128 x 128 |
| [ ] | `ui_swatch_selected.png` | Frame round the chosen swatch | 128 x 128 |
| [ ] | `ui_portrait_ring.png` | Ring round a round portrait (journal, top bar, map pins) | 256 x 256 |
| [ ] | `ui_scrollbar.png` | Scroll bar handle (9-slice) | 64 x 256 |

Fonts: send the `.ttf` / `.otf` files as they are (e.g. `font_headings.ttf`, `font_body.ttf`); check the licence allows a
game you sell.

### 1c. Top bar and shop furniture UI (folder `Art/UI/`)
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `ui_topbar.png` | The bar across the top (9-slice) | 2048 x 220 |
| [ ] | `ui_weekday_badge.png` | Round badge behind the weekday symbol (tinted per day) | 256 x 256 |
| [ ] | `ui_coin_counter.png` | Box behind the coin count (9-slice) | 512 x 160 |
| [ ] | `ui_clock_face.png` | The clock in the top bar | 256 x 256 |
| [ ] | `ui_xp_frame.png` | Personal area: frame for portrait, name and XP bar | 768 x 256 |
| [ ] | `ui_sign_open.png` | Shop sign, OPEN | 512 x 256 |
| [ ] | `ui_sign_closed.png` | Shop sign, CLOSED | 512 x 256 |
| [ ] | `ui_page_dot.png` | Page dot (shop pages) | 64 x 64 |
| [ ] | `ui_page_dot_on.png` | Page dot, current page | 64 x 64 |
| [ ] | `ui_decor_spot.png` | Glowing outline of a decor spot while decorating | 512 x 512 |
| [ ] | `ui_decor_tray.png` | The decorating tray along the bottom (9-slice) | 2048 x 512 |

### 1d. Icons (folder `Art/UI/`, transparent; draw them white or in ink, as noted)
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `icon_gear.png` | Settings (replaces placeholder; white) | 256 x 256 |
| [ ] | `icon_pencil.png` | Decorate the shop (replaces placeholder) | 256 x 256 |
| [ ] | `icon_coin.png` | Coin | 128 x 128 |
| [ ] | `icon_xp.png` | Experience star | 128 x 128 |
| [ ] | `icon_heart_friend.png` | Friendship heart | 128 x 128 |
| [ ] | `icon_heart_romance.png` | Romance heart | 128 x 128 |
| [ ] | `icon_heart_empty.png` | Romance heart, not yet filled | 128 x 128 |
| [ ] | `icon_bell.png` | Notices bell | 128 x 128 |
| [ ] | `icon_doorbell.png` | Doorbell (a customer is here) | 128 x 128 |
| [ ] | `icon_clock.png` | Clock | 128 x 128 |
| [ ] | `icon_calendar.png` | Calendar | 128 x 128 |
| [ ] | `icon_journal.png` | Journal | 128 x 128 |
| [ ] | `icon_shops.png` | Shops menu (bag) | 128 x 128 |
| [ ] | `icon_close.png` | Close (x) | 128 x 128 |
| [ ] | `icon_back.png` | Back arrow | 128 x 128 |
| [ ] | `icon_undo.png` | Undo | 128 x 128 |
| [ ] | `icon_seam_ripper.png` | Stitch picker | 128 x 128 |
| [ ] | `icon_zoom_in.png` | Zoom in (crafting) | 128 x 128 |
| [ ] | `icon_zoom_out.png` | Zoom out (crafting) | 128 x 128 |
| [ ] | `icon_turn.png` | Turn the work (crafting) | 128 x 128 |
| [ ] | `icon_lock.png` | Locked | 128 x 128 |
| [ ] | `icon_tick.png` | Done / ticked | 128 x 128 |
| [ ] | `icon_cake.png` | Birthday | 128 x 128 |
| [ ] | `icon_star_full.png` | Quality star, earned | 128 x 128 |
| [ ] | `icon_star_empty.png` | Quality star, not earned | 128 x 128 |
| [ ] | `icon_eraser.png` | Eraser (pattern designer) | 128 x 128 |
| [ ] | `icon_copy.png` | Copy a trade code | 128 x 128 |

Weekday symbols (`Art/UI/Weekdays/`, replace the white placeholders; draw them WHITE, the game tints the badge):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `weekday_1_monday.png` | Monday | 256 x 256 |
| [ ] | `weekday_2_tuesday.png` | Tuesday | 256 x 256 |
| [ ] | `weekday_3_wednesday.png` | Wednesday | 256 x 256 |
| [ ] | `weekday_4_thursday.png` | Thursday | 256 x 256 |
| [ ] | `weekday_5_friday.png` | Friday | 256 x 256 |
| [ ] | `weekday_6_saturday.png` | Saturday | 256 x 256 |
| [ ] | `weekday_7_sunday.png` | Sunday | 256 x 256 |

Map pins (`Art/UI/`):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `pin_place.png` | A place in town | 256 x 256 |
| [ ] | `pin_shop.png` | A shop | 256 x 256 |
| [ ] | `pin_shut.png` | Somewhere you can't go right now | 256 x 256 |
| [ ] | `pin_home.png` | Your own shop | 256 x 256 |
| [ ] | `pin_plaque.png` | Name plaque under a pin (9-slice) | 512 x 160 |
| [ ] | `map_card.png` | The card that pops up from a pin (9-slice) | 512 x 512 |

Stamps (achievements, `Art/UI/Stamps/`): a frame per theme, drawn white with ink detail (the game tints it):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `stamp_craft.png` | Craft stamp | 512 x 512 |
| [ ] | `stamp_skill.png` | Skill stamp | 512 x 512 |
| [ ] | `stamp_friendship.png` | Friendship stamp | 512 x 512 |
| [ ] | `stamp_romance.png` | Romance stamp | 512 x 512 |
| [ ] | `stamp_story.png` | Story stamp | 512 x 512 |
| [ ] | `stamp_shop.png` | Shop stamp | 512 x 512 |

### 1e. Logo and app icon
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `logo_title.png` | Title logo (`Art/Logo/`), transparent | 2048 wide |
| [ ] | `app_icon.png` | App icon (`Art/Logo/`), NOT transparent, shapes away from the corners | 1024 x 1024 |

### 1f. Animations and transitions (`Art/UI/Animations/`)
A few frames each (a short flipbook), numbered from 01. Or just a "before" and an "after" picture (frames 01 and 02)
and Claude cross-fades them. Same size as a background.
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `anim_day_to_night_01.png ... _06.png` | Closing the shop: light goes, sign flips, workroom lamps come on | 2048 x 4440 |
| [ ] | `anim_night_to_morning_01.png ... _06.png` | Going to bed, the next morning | 2048 x 4440 |
| [ ] | `anim_door_open_01.png ... _04.png` | The shop door swinging open | 1024 x 2048 |
| [ ] | `anim_journal_open_01.png ... _04.png` | The journal opening | 2048 x 2732 |
| [ ] | `anim_page_turn_01.png ... _04.png` | A journal page turning | 2048 x 2732 |
| [ ] | `anim_stamp_press_01.png ... _04.png` | A stamp pressed into the album | 512 x 512 |
| [ ] | `anim_level_up_01.png ... _04.png` | Level up / a new craft learned (sparkle) | 1024 x 1024 |

## Part 2: Backgrounds (folder `Art/Backgrounds/`)

Keep anything important inside the middle 2048 x 2732; the top and bottom only show on tall phones.
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `bg_shop_day.png` | The shop by day (replaces placeholder) | 2048 x 4440 |
| [ ] | `bg_workroom_night.png` | The workroom at night (replaces placeholder) | 2048 x 4440 |
| [ ] | `bg_workbench.png` | Workbench close-up, behind the crafting (replaces placeholder) | 2048 x 4440 |
| [ ] | `town_map.png` | Saturday town map; sea along the bottom (replaces placeholder; pins at ShopDay.MapSpot) | 2048 x 3600 |
| [ ] | `bg_title.png` | Title screen (new) | 2048 x 4440 |
| [ ] | `bg_shop_pickups.png` | Shop, left page: the pickup corner (new) | 2048 x 4440 |
| [ ] | `bg_shop_display.png` | Shop, right page: the pattern display (new) | 2048 x 4440 |
| [ ] | `bg_market.png` | The Supply Market (new) | 2048 x 4440 |
| [ ] | `bg_boutique.png` | Moreau's Boutique (new) | 2048 x 4440 |
| [ ] | `bg_pattern_shop.png` | Finch's Pattern Parlour (new) | 2048 x 4440 |
| [ ] | `bg_workshop.png` | Marsh's Workshop (new) | 2048 x 4440 |
| [ ] | `bg_curios.png` | Quill's Curiosities (new) | 2048 x 4440 |
| [ ] | `bg_tech_shop.png` | Byte & Bobbin (new) | 2048 x 4440 |
| [ ] | `bg_stall_square.png` | Sunday stall, the Square Market (new) | 2048 x 4440 |
| [ ] | `bg_stall_fair.png` | Sunday stall, the Harbour Fair (new) | 2048 x 4440 |
| [ ] | `bg_attic.png` | The attic (shop expansion) (new) | 2048 x 4440 |
| [ ] | `bg_basement.png` | The basement (shop expansion) (new) | 2048 x 4440 |
| [ ] | `bg_journal.png` | The open journal (folder `Art/UI/`) (new) | 2048 x 2732 |

Optional, later: a background per Saturday place (new), `bg_place_<id>.png`, 2048 x 4440, for `bg_place_harbour.png`, `bg_place_lighthouse.png`, `bg_place_beach.png`, `bg_place_cafe.png`, `bg_place_tearoom.png`, `bg_place_library.png`, `bg_place_bookshop.png`, `bg_place_church.png`, `bg_place_florist.png`, `bg_place_studio.png`, `bg_place_hospital.png`, `bg_place_school.png`, `bg_place_park.png`, `bg_place_homes.png`, `bg_place_knot.png`.

## Part 3: Characters (folder `Art/Characters/Customers/`, all replace placeholders)

Each character: **1 full body** (1500 x 2000, a little space under the feet) and **5 talking heads** (1024 x 1024, the
head in the same place on each). 6 drawings each, 150 in all.

**Mara** (new parent)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_mara_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_mara_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_mara_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_mara_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_mara_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_mara_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Old Tobias** (fisherman)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_tobias_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_tobias_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_tobias_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_tobias_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_tobias_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_tobias_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Juno** (teen, knitwear)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_juno_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_juno_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_juno_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_juno_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_juno_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_juno_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Priya Nair** (teacher)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_priya_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_priya_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_priya_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_priya_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_priya_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_priya_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Hal Penrose** (retired postman)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_hal_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_hal_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_hal_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_hal_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_hal_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_hal_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Isla Marlowe** (lighthouse keeper (romance))

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_isla_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_isla_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_isla_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_isla_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_isla_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_isla_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Eli Brooks** (Driftwood Café (romance))

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_eli_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_eli_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_eli_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_eli_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_eli_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_eli_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Nadia Kowalski** (midwife)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_nadia_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_nadia_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_nadia_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_nadia_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_nadia_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_nadia_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Sol Reyes** (lifeguard)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_sol_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_sol_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_sol_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_sol_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_sol_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_sol_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Gwen Tallow** (harbourmaster)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_gwen_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_gwen_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_gwen_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_gwen_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_gwen_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_gwen_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Midge Lowe** (Ropewalk Books)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_midge_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_midge_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_midge_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_midge_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_midge_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_midge_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Theo Lindqvist** (Tobias's grandson)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_theo_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_theo_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_theo_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_theo_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_theo_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_theo_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Felix Adeyemi** (florist)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_felix_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_felix_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_felix_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_felix_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_felix_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_felix_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Dot Pascoe** (tea room)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_dot_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_dot_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_dot_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_dot_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_dot_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_dot_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Benji Marsh** (carpenter, Marsh's Workshop)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_benji_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_benji_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_benji_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_benji_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_benji_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_benji_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Sam** (music teacher)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_sam_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_sam_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_sam_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_sam_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_sam_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_sam_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Owen Price** (vicar)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_owen_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_owen_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_owen_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_owen_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_owen_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_owen_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Wenna Trelawney** (quilting bee)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_wenna_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_wenna_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_wenna_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_wenna_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_wenna_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_wenna_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Kit Calloway** (painter)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_kit_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_kit_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_kit_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_kit_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_kit_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_kit_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Rowan Hale** (Knot & Needle (romance))

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_rowan_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_rowan_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_rowan_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_rowan_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_rowan_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_rowan_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Bram Okafor** (Supply Market (romance))

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_bram_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_bram_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_bram_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_bram_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_bram_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_bram_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Celeste Moreau** (Boutique (romance))

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_celeste_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_celeste_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_celeste_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_celeste_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_celeste_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_celeste_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Ada Finch** (Pattern Parlour)

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_ada_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_ada_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_ada_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_ada_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_ada_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_ada_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Jasper Quill** (Curiosities (romance))

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_jasper_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_jasper_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_jasper_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_jasper_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_jasper_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_jasper_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

**Ravi Sen** (Byte & Bobbin (romance))

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `customer_ravi_body.png` | Full body | 1500 x 2000 |
| [ ] | `customer_ravi_neutral.png` | Talking head, neutral | 1024 x 1024 |
| [ ] | `customer_ravi_happy.png` | Talking head, happy | 1024 x 1024 |
| [ ] | `customer_ravi_sad.png` | Talking head, sad | 1024 x 1024 |
| [ ] | `customer_ravi_surprised.png` | Talking head, surprised | 1024 x 1024 |
| [ ] | `customer_ravi_thoughtful.png` | Talking head, thoughtful | 1024 x 1024 |

The Harbour Fair regulars (new, talking heads only, 1024 x 1024): `fair_marguerite.png`, `fair_tam.png`, `fair_achterberg.png`,
`fair_bea.png`. Strangers at the stall can share a few generic faces: `stranger_01.png` ... `stranger_08.png`.

## Part 4: The player character (folder `Art/Characters/Player/`, all new)

Separate drawings for every option and colour (your choice: no tinting). Draw every layer on the **same 1500 x 2000
canvas**, lined up, so they stack: body, then top, then hair, then accessories. Plus the face crop for the round portrait.

**Body / skin (6)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_body_skin1_lightest.png` | Skin tone 1 | 1500 x 2000 |
| [ ] | `player_body_skin2_light.png` | Skin tone 2 | 1500 x 2000 |
| [ ] | `player_body_skin3_medium_light.png` | Skin tone 3 | 1500 x 2000 |
| [ ] | `player_body_skin4_medium.png` | Skin tone 4 | 1500 x 2000 |
| [ ] | `player_body_skin5_medium_dark.png` | Skin tone 5 | 1500 x 2000 |
| [ ] | `player_body_skin6_darkest.png` | Skin tone 6 | 1500 x 2000 |

**Hair: Short (8 colours)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_hair_short_black.png` | Short, black | 1500 x 2000 |
| [ ] | `player_hair_short_brown.png` | Short, brown | 1500 x 2000 |
| [ ] | `player_hair_short_chestnut.png` | Short, chestnut | 1500 x 2000 |
| [ ] | `player_hair_short_blonde.png` | Short, blonde | 1500 x 2000 |
| [ ] | `player_hair_short_red.png` | Short, red | 1500 x 2000 |
| [ ] | `player_hair_short_grey.png` | Short, grey | 1500 x 2000 |
| [ ] | `player_hair_short_pink.png` | Short, pink | 1500 x 2000 |
| [ ] | `player_hair_short_blue.png` | Short, blue | 1500 x 2000 |

**Hair: Bob (8 colours)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_hair_bob_black.png` | Bob, black | 1500 x 2000 |
| [ ] | `player_hair_bob_brown.png` | Bob, brown | 1500 x 2000 |
| [ ] | `player_hair_bob_chestnut.png` | Bob, chestnut | 1500 x 2000 |
| [ ] | `player_hair_bob_blonde.png` | Bob, blonde | 1500 x 2000 |
| [ ] | `player_hair_bob_red.png` | Bob, red | 1500 x 2000 |
| [ ] | `player_hair_bob_grey.png` | Bob, grey | 1500 x 2000 |
| [ ] | `player_hair_bob_pink.png` | Bob, pink | 1500 x 2000 |
| [ ] | `player_hair_bob_blue.png` | Bob, blue | 1500 x 2000 |

**Hair: Long (8 colours)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_hair_long_black.png` | Long, black | 1500 x 2000 |
| [ ] | `player_hair_long_brown.png` | Long, brown | 1500 x 2000 |
| [ ] | `player_hair_long_chestnut.png` | Long, chestnut | 1500 x 2000 |
| [ ] | `player_hair_long_blonde.png` | Long, blonde | 1500 x 2000 |
| [ ] | `player_hair_long_red.png` | Long, red | 1500 x 2000 |
| [ ] | `player_hair_long_grey.png` | Long, grey | 1500 x 2000 |
| [ ] | `player_hair_long_pink.png` | Long, pink | 1500 x 2000 |
| [ ] | `player_hair_long_blue.png` | Long, blue | 1500 x 2000 |

**Hair: Top Knot (boutique) (8 colours)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_hair_bun_black.png` | Top Knot (boutique), black | 1500 x 2000 |
| [ ] | `player_hair_bun_brown.png` | Top Knot (boutique), brown | 1500 x 2000 |
| [ ] | `player_hair_bun_chestnut.png` | Top Knot (boutique), chestnut | 1500 x 2000 |
| [ ] | `player_hair_bun_blonde.png` | Top Knot (boutique), blonde | 1500 x 2000 |
| [ ] | `player_hair_bun_red.png` | Top Knot (boutique), red | 1500 x 2000 |
| [ ] | `player_hair_bun_grey.png` | Top Knot (boutique), grey | 1500 x 2000 |
| [ ] | `player_hair_bun_pink.png` | Top Knot (boutique), pink | 1500 x 2000 |
| [ ] | `player_hair_bun_blue.png` | Top Knot (boutique), blue | 1500 x 2000 |

**Hair: Big Curls (boutique) (8 colours)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_hair_curly_black.png` | Big Curls (boutique), black | 1500 x 2000 |
| [ ] | `player_hair_curly_brown.png` | Big Curls (boutique), brown | 1500 x 2000 |
| [ ] | `player_hair_curly_chestnut.png` | Big Curls (boutique), chestnut | 1500 x 2000 |
| [ ] | `player_hair_curly_blonde.png` | Big Curls (boutique), blonde | 1500 x 2000 |
| [ ] | `player_hair_curly_red.png` | Big Curls (boutique), red | 1500 x 2000 |
| [ ] | `player_hair_curly_grey.png` | Big Curls (boutique), grey | 1500 x 2000 |
| [ ] | `player_hair_curly_pink.png` | Big Curls (boutique), pink | 1500 x 2000 |
| [ ] | `player_hair_curly_blue.png` | Big Curls (boutique), blue | 1500 x 2000 |

**Top: Jumper (8 colours)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_top_jumper_blue.png` | Jumper, blue | 1500 x 2000 |
| [ ] | `player_top_jumper_red.png` | Jumper, red | 1500 x 2000 |
| [ ] | `player_top_jumper_green.png` | Jumper, green | 1500 x 2000 |
| [ ] | `player_top_jumper_yellow.png` | Jumper, yellow | 1500 x 2000 |
| [ ] | `player_top_jumper_purple.png` | Jumper, purple | 1500 x 2000 |
| [ ] | `player_top_jumper_charcoal.png` | Jumper, charcoal | 1500 x 2000 |
| [ ] | `player_top_jumper_white.png` | Jumper, white | 1500 x 2000 |
| [ ] | `player_top_jumper_tan.png` | Jumper, tan | 1500 x 2000 |

**Top: Apron (8 colours)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_top_apron_blue.png` | Apron, blue | 1500 x 2000 |
| [ ] | `player_top_apron_red.png` | Apron, red | 1500 x 2000 |
| [ ] | `player_top_apron_green.png` | Apron, green | 1500 x 2000 |
| [ ] | `player_top_apron_yellow.png` | Apron, yellow | 1500 x 2000 |
| [ ] | `player_top_apron_purple.png` | Apron, purple | 1500 x 2000 |
| [ ] | `player_top_apron_charcoal.png` | Apron, charcoal | 1500 x 2000 |
| [ ] | `player_top_apron_white.png` | Apron, white | 1500 x 2000 |
| [ ] | `player_top_apron_tan.png` | Apron, tan | 1500 x 2000 |

**Top: Cosy Cardigan (boutique) (8 colours)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_top_cardigan_blue.png` | Cosy Cardigan (boutique), blue | 1500 x 2000 |
| [ ] | `player_top_cardigan_red.png` | Cosy Cardigan (boutique), red | 1500 x 2000 |
| [ ] | `player_top_cardigan_green.png` | Cosy Cardigan (boutique), green | 1500 x 2000 |
| [ ] | `player_top_cardigan_yellow.png` | Cosy Cardigan (boutique), yellow | 1500 x 2000 |
| [ ] | `player_top_cardigan_purple.png` | Cosy Cardigan (boutique), purple | 1500 x 2000 |
| [ ] | `player_top_cardigan_charcoal.png` | Cosy Cardigan (boutique), charcoal | 1500 x 2000 |
| [ ] | `player_top_cardigan_white.png` | Cosy Cardigan (boutique), white | 1500 x 2000 |
| [ ] | `player_top_cardigan_tan.png` | Cosy Cardigan (boutique), tan | 1500 x 2000 |

**Accessories, things you made, face**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `player_acc_brooch.png` | Daisy Brooch | 1500 x 2000 |
| [ ] | `player_acc_glasses.png` | Round Glasses | 1500 x 2000 |
| [ ] | `player_acc_hat.png` | Felt Hat | 1500 x 2000 |
| [ ] | `player_acc_neckerchief.png` | Sailor Neckerchief | 1500 x 2000 |
| [ ] | `player_made_sweater.png` | A sweater you knitted (drawn light grey; the game colours it with the yarn) | 1500 x 2000 |
| [ ] | `player_made_scarf.png` | A scarf you knitted (light grey) | 1500 x 2000 |
| [ ] | `player_made_cable_scarf.png` | A cable scarf you knitted (light grey) | 1500 x 2000 |
| [ ] | `player_face_neutral.png` | Face, neutral | 1500 x 2000 |
| [ ] | `player_face_happy.png` | Face, happy | 1500 x 2000 |

Boutique tiles (folder `Art/Cosmetics/`, replace placeholders):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `cosmetic_acc_brooch.png` | Daisy Brooch | 512 x 512 |
| [ ] | `cosmetic_acc_glasses.png` | Round Glasses | 512 x 512 |
| [ ] | `cosmetic_acc_hat.png` | Felt Hat | 512 x 512 |
| [ ] | `cosmetic_acc_neckerchief.png` | Sailor Neckerchief | 512 x 512 |
| [ ] | `cosmetic_hair_bun.png` | Top Knot | 512 x 512 |
| [ ] | `cosmetic_hair_curly.png` | Big Curls | 512 x 512 |
| [ ] | `cosmetic_top_cardigan.png` | Cosy Cardigan | 512 x 512 |

## Part 5: Items (folder `Art/Items/`)

Icons replace placeholders; the finished pictures are new. Draw plain (white/grey) where the pattern's colours go: the game lays the pattern on top.
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `item_sampler_icon.png` | Sampler: icon | 512 x 512 |
| [ ] | `item_sampler_finished.png` | Sampler: finished picture (new) | 1024 x 1024 |
| [ ] | `item_tablecloth_icon.png` | Tablecloth: icon | 512 x 512 |
| [ ] | `item_tablecloth_finished.png` | Tablecloth: finished picture (new) | 1024 x 1024 |
| [ ] | `item_scarf_icon.png` | Scarf: icon | 512 x 512 |
| [ ] | `item_scarf_finished.png` | Scarf: finished picture (new) | 1024 x 1024 |
| [ ] | `item_cable_scarf_icon.png` | Cable-Knit Scarf: icon | 512 x 512 |
| [ ] | `item_cable_scarf_finished.png` | Cable-Knit Scarf: finished picture (new) | 1024 x 1024 |
| [ ] | `item_sweater_icon.png` | Sweater: icon | 512 x 512 |
| [ ] | `item_sweater_finished.png` | Sweater: finished picture (new) | 1024 x 1024 |
| [ ] | `item_hoop_art_icon.png` | Embroidered Hoop: icon | 512 x 512 |
| [ ] | `item_hoop_art_finished.png` | Embroidered Hoop: finished picture (new) | 1024 x 1024 |
| [ ] | `item_plushie_icon.png` | Crocheted Plushie: icon | 512 x 512 |
| [ ] | `item_plushie_finished.png` | Crocheted Plushie: finished picture (new) | 1024 x 1024 |
| [ ] | `item_plushie_sewn_icon.png` | Sewn Plushie: icon | 512 x 512 |
| [ ] | `item_plushie_sewn_finished.png` | Sewn Plushie: finished picture (new) | 1024 x 1024 |
| [ ] | `item_tote_icon.png` | Tote Bag: icon | 512 x 512 |
| [ ] | `item_tote_finished.png` | Tote Bag: finished picture (new) | 1024 x 1024 |
| [ ] | `item_cushion_icon.png` | Needlepoint Cushion: icon | 512 x 512 |
| [ ] | `item_cushion_finished.png` | Needlepoint Cushion: finished picture (new) | 1024 x 1024 |
| [ ] | `item_quilt_icon.png` | Patchwork Quilt: icon | 512 x 512 |
| [ ] | `item_quilt_finished.png` | Patchwork Quilt: finished picture (new) | 1024 x 1024 |
| [ ] | `item_wall_hanging_icon.png` | Woven Wall Hanging: icon | 512 x 512 |
| [ ] | `item_wall_hanging_finished.png` | Woven Wall Hanging: finished picture (new) | 1024 x 1024 |

Patterns are charts the game draws, so they need no art. Optional (new, `Art/UI/`): `ui_pattern_leaflet.png` (a printed
pattern's cover frame, 1024 x 1024) and `ui_magazine_cover.png` (1024 x 1400).

## Part 6: Crafting (all new)

Tools in the hand (folder `Art/Crafts/Tools/`, 256 x 256, transparent, pointing up-left):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `tool_needle_tapestry.png` | Tapestry needle | 256 x 256 |
| [ ] | `tool_needle_embroidery.png` | Embroidery needle | 256 x 256 |
| [ ] | `tool_needle_sewing.png` | Sewing needle | 256 x 256 |
| [ ] | `tool_crochet_hook.png` | Crochet hook | 256 x 256 |
| [ ] | `tool_knitting_needles.png` | Pair of knitting needles | 256 x 256 |
| [ ] | `tool_seam_ripper.png` | Seam ripper | 256 x 256 |
| [ ] | `tool_iron.png` | Iron | 256 x 256 |
| [ ] | `tool_machine_foot.png` | Sewing machine foot and needle | 256 x 256 |
| [ ] | `tool_shuttle.png` | Weaving shuttle | 256 x 256 |
| [ ] | `tool_beater.png` | Weaving beater | 256 x 256 |
| [ ] | `tool_rotary_cutter.png` | Rotary cutter | 256 x 256 |
| [ ] | `tool_scissors.png` | Fabric scissors | 256 x 256 |
| [ ] | `tool_pin.png` | Pin | 256 x 256 |
| [ ] | `tool_marking_pen.png` | Marking pen | 256 x 256 |

Stitches (folder `Art/Crafts/Stitches/`, 256 x 256, drawn WHITE: the game colours them with the thread):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `stitch_cross_half.png` | Cross stitch: half stitch "/" | 256 x 256 |
| [ ] | `stitch_cross_half_back.png` | Cross stitch: half stitch "\\" | 256 x 256 |
| [ ] | `stitch_cross_full.png` | Cross stitch: full X, "\\" on top | 256 x 256 |
| [ ] | `stitch_cross_full_flipped.png` | Cross stitch: full X, "/" on top | 256 x 256 |
| [ ] | `stitch_cross_three_quarter.png` | Three-quarter stitch | 256 x 256 |
| [ ] | `stitch_tent.png` | Needlepoint tent stitch | 256 x 256 |
| [ ] | `stitch_backstitch.png` | Backstitch / running stitch segment | 256 x 256 |
| [ ] | `stitch_satin.png` | Satin stitch strand | 256 x 256 |
| [ ] | `stitch_stem.png` | Stem stitch | 256 x 256 |
| [ ] | `stitch_french_knot.png` | French knot | 256 x 256 |
| [ ] | `stitch_knit.png` | Knit stitch (V) | 256 x 256 |
| [ ] | `stitch_purl.png` | Purl bump | 256 x 256 |
| [ ] | `stitch_cable_left.png` | Cable cross, to the left | 256 x 256 |
| [ ] | `stitch_cable_right.png` | Cable cross, to the right | 256 x 256 |
| [ ] | `stitch_dropped.png` | Dropped stitch ladder | 256 x 256 |
| [ ] | `stitch_crochet_sc.png` | Crochet single crochet | 256 x 256 |
| [ ] | `stitch_crochet_inc.png` | Crochet increase | 256 x 256 |
| [ ] | `stitch_crochet_dec.png` | Crochet decrease | 256 x 256 |
| [ ] | `stitch_magic_ring.png` | Magic ring | 256 x 256 |
| [ ] | `stitch_machine.png` | Machine stitch dash | 256 x 256 |
| [ ] | `stitch_hand.png` | Hand-sewn stitch dash | 256 x 256 |
| [ ] | `stitch_ladder.png` | Ladder stitch | 256 x 256 |
| [ ] | `stitch_weft.png` | Weaving: weft over/under | 256 x 256 |
| [ ] | `stitch_warp.png` | Weaving: warp thread | 256 x 256 |

Surfaces (folder `Art/Textures/`, tileable, 1024 x 1024, no transparency):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `tex_aida_white.png` | Aida, white | 1024 x 1024 |
| [ ] | `tex_aida_cream.png` | Aida, cream | 1024 x 1024 |
| [ ] | `tex_linen.png` | Linen evenweave | 1024 x 1024 |
| [ ] | `tex_needlepoint_canvas.png` | Needlepoint canvas | 1024 x 1024 |
| [ ] | `tex_felt.png` | Felt | 1024 x 1024 |
| [ ] | `tex_quilting_cotton.png` | Quilting cotton | 1024 x 1024 |
| [ ] | `tex_minky.png` | Minky | 1024 x 1024 |
| [ ] | `tex_fleece.png` | Fleece | 1024 x 1024 |
| [ ] | `tex_canvas.png` | Canvas (tote) | 1024 x 1024 |
| [ ] | `tex_knit_wool.png` | Knitted wool | 1024 x 1024 |
| [ ] | `tex_crochet.png` | Crocheted fabric | 1024 x 1024 |
| [ ] | `tex_woven.png` | Woven cloth | 1024 x 1024 |
| [ ] | `tex_pattern_paper.png` | Pattern paper | 1024 x 1024 |
| [ ] | `tex_wood.png` | Wooden workbench | 1024 x 1024 |

Frames and small parts (folder `Art/Crafts/Frames/`, transparent):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `frame_hoop.png` | Embroidery hoop: wooden ring and screw | 1024 x 1024 |
| [ ] | `frame_qsnap.png` | Q-Snap frame | 1024 x 1024 |
| [ ] | `frame_stretcher_bars.png` | Stretcher bars with brass tacks | 1024 x 1024 |
| [ ] | `frame_loom.png` | Frame loom with pegs | 1024 x 1024 |
| [ ] | `frame_ironing_board.png` | Ironing board | 1024 x 512 |
| [ ] | `frame_heat_dial.png` | Iron heat dial | 256 x 256 |
| [ ] | `frame_sewing_machine.png` | Sewing machine, side view | 1024 x 1024 |
| [ ] | `part_stuffing.png` | Tuft of stuffing | 256 x 256 |
| [ ] | `part_safety_eye.png` | Safety eye | 256 x 256 |
| [ ] | `part_button.png` | Button | 256 x 256 |
| [ ] | `part_pin_head.png` | Pin head | 256 x 256 |
| [ ] | `part_safety_pin.png` | Safety pin | 256 x 256 |
| [ ] | `part_tack.png` | Brass tack | 256 x 256 |
| [ ] | `part_yarn_ball.png` | Ball of yarn in hand (colourwork; drawn white) | 256 x 256 |

## Part 7: Supplies (folder `Art/Supplies/`, all replace placeholders, 512 x 512)

Shown on the Supply Market shelves. Many are one drawing recoloured (one skein of floss, one ball of yarn).

**Floss and thread (34)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `supply_floss_black.png` | Black Floss | 512 x 512 |
| [ ] | `supply_floss_brown.png` | Brown Floss | 512 x 512 |
| [ ] | `supply_floss_burgundy.png` | Burgundy Floss | 512 x 512 |
| [ ] | `supply_floss_charcoal.png` | Charcoal Floss | 512 x 512 |
| [ ] | `supply_floss_coral.png` | Coral Floss | 512 x 512 |
| [ ] | `supply_floss_cream.png` | Cream Floss | 512 x 512 |
| [ ] | `supply_floss_forest.png` | Forest Green Floss | 512 x 512 |
| [ ] | `supply_floss_gold.png` | Gold Floss | 512 x 512 |
| [ ] | `supply_floss_green.png` | Green Floss | 512 x 512 |
| [ ] | `supply_floss_lavender.png` | Lavender Floss | 512 x 512 |
| [ ] | `supply_floss_lemon.png` | Lemon Floss | 512 x 512 |
| [ ] | `supply_floss_magenta.png` | Magenta Floss | 512 x 512 |
| [ ] | `supply_floss_metallic_gold.png` | Metallic Gold Floss | 512 x 512 |
| [ ] | `supply_floss_metallic_silver.png` | Metallic Silver Floss | 512 x 512 |
| [ ] | `supply_floss_mint.png` | Mint Floss | 512 x 512 |
| [ ] | `supply_floss_navy.png` | Navy Floss | 512 x 512 |
| [ ] | `supply_floss_olive.png` | Olive Floss | 512 x 512 |
| [ ] | `supply_floss_orange.png` | Orange Floss | 512 x 512 |
| [ ] | `supply_floss_peach.png` | Peach Floss | 512 x 512 |
| [ ] | `supply_floss_periwinkle.png` | Periwinkle Floss | 512 x 512 |
| [ ] | `supply_floss_pink.png` | Pink Floss | 512 x 512 |
| [ ] | `supply_floss_plum.png` | Plum Floss | 512 x 512 |
| [ ] | `supply_floss_purple.png` | Purple Floss | 512 x 512 |
| [ ] | `supply_floss_red.png` | Red Floss | 512 x 512 |
| [ ] | `supply_floss_rose.png` | Dusty Rose Floss | 512 x 512 |
| [ ] | `supply_floss_royal.png` | Royal Blue Floss | 512 x 512 |
| [ ] | `supply_floss_rust.png` | Rust Floss | 512 x 512 |
| [ ] | `supply_floss_silver_grey.png` | Silver Grey Floss | 512 x 512 |
| [ ] | `supply_floss_sky.png` | Sky Blue Floss | 512 x 512 |
| [ ] | `supply_floss_tan.png` | Tan Floss | 512 x 512 |
| [ ] | `supply_floss_teal.png` | Teal Floss | 512 x 512 |
| [ ] | `supply_floss_turquoise.png` | Turquoise Floss | 512 x 512 |
| [ ] | `supply_floss_white.png` | White Floss | 512 x 512 |
| [ ] | `supply_floss_yellow.png` | Sunny Yellow Floss | 512 x 512 |

**Yarn (18)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `supply_acrylic_yarn.png` | Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_black.png` | Black Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_brown.png` | Brown Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_green.png` | Green Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_grey.png` | Grey Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_navy.png` | Navy Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_orange.png` | Orange Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_pink.png` | Pink Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_purple.png` | Purple Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_red.png` | Red Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_sky.png` | Sky Blue Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_white.png` | White Acrylic Yarn | 512 x 512 |
| [ ] | `supply_acrylic_yarn_yellow.png` | Yellow Acrylic Yarn | 512 x 512 |
| [ ] | `supply_alpaca_yarn.png` | 100% Alpaca Yarn | 512 x 512 |
| [ ] | `supply_cotton_yarn.png` | Cotton Yarn | 512 x 512 |
| [ ] | `supply_mohair_yarn.png` | Mohair Yarn | 512 x 512 |
| [ ] | `supply_wool_basic.png` | Basic Wool | 512 x 512 |
| [ ] | `supply_wool_merino.png` | Merino Wool | 512 x 512 |

**Fabric (14)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `supply_aida_14ct.png` | Aida 14ct | 512 x 512 |
| [ ] | `supply_aida_18ct.png` | Aida 18ct | 512 x 512 |
| [ ] | `supply_aida_cream.png` | Cream Aida 14ct | 512 x 512 |
| [ ] | `supply_canvas_natural.png` | Natural Canvas | 512 x 512 |
| [ ] | `supply_embroidery_cotton.png` | White Embroidery Cotton | 512 x 512 |
| [ ] | `supply_felt_brown.png` | Brown Felt | 512 x 512 |
| [ ] | `supply_felt_cream.png` | Cream Felt | 512 x 512 |
| [ ] | `supply_fleece_blue.png` | Blue Fleece | 512 x 512 |
| [ ] | `supply_linen_28ct.png` | Linen Evenweave 28ct | 512 x 512 |
| [ ] | `supply_minky_pink.png` | Pink Minky | 512 x 512 |
| [ ] | `supply_mono_canvas.png` | Mono Canvas 14ct | 512 x 512 |
| [ ] | `supply_quilting_cotton.png` | Quilting Cotton | 512 x 512 |
| [ ] | `supply_quilting_floral.png` | Floral Quilting Cotton | 512 x 512 |
| [ ] | `supply_velvet_backing.png` | Velvet Cushion Backing | 512 x 512 |

**Notions (13)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `supply_buttons_white.png` | White Buttons | 512 x 512 |
| [ ] | `supply_buttons_wood.png` | Wooden Buttons | 512 x 512 |
| [ ] | `supply_cushion_pad.png` | Cushion Pad | 512 x 512 |
| [ ] | `supply_dowel.png` | Wooden Dowel | 512 x 512 |
| [ ] | `supply_interfacing.png` | Interfacing | 512 x 512 |
| [ ] | `supply_lace_trim.png` | Cotton Lace Trim | 512 x 512 |
| [ ] | `supply_masking_tape.png` | Masking Tape | 512 x 512 |
| [ ] | `supply_ribbon_satin.png` | Satin Ribbon | 512 x 512 |
| [ ] | `supply_wadding.png` | Cotton Wadding | 512 x 512 |
| [ ] | `supply_warp_cotton.png` | Cotton Warp Thread | 512 x 512 |
| [ ] | `supply_weaving_yarn.png` | Weaving Yarn Bundle | 512 x 512 |
| [ ] | `supply_webbing.png` | Cotton Webbing | 512 x 512 |
| [ ] | `supply_zip_20cm.png` | 20cm Zip | 512 x 512 |

**Stuffing (1)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `supply_stuffing.png` | Polyester Stuffing | 512 x 512 |

**Safety eyes (1)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `supply_safety_eyes_black.png` | Black Safety Eyes | 512 x 512 |

**Tools (26)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `supply_crochet_hook_5mm.png` | 5mm Crochet Hook | 512 x 512 |
| [ ] | `supply_crochet_hook_ergo_5mm.png` | 5mm Soft-Grip Crochet Hook | 512 x 512 |
| [ ] | `supply_embroidery_needle.png` | Embroidery Needle | 512 x 512 |
| [ ] | `supply_embroidery_needle_gold.png` | Gold-Eye Embroidery Needles | 512 x 512 |
| [ ] | `supply_fabric_scissors.png` | Fabric Scissors | 512 x 512 |
| [ ] | `supply_frame_loom.png` | Frame Loom | 512 x 512 |
| [ ] | `supply_hoop_4in.png` | 4" Embroidery Hoop | 512 x 512 |
| [ ] | `supply_hoop_6in.png` | 6" Embroidery Hoop | 512 x 512 |
| [ ] | `supply_hoop_8in.png` | 8" Embroidery Hoop | 512 x 512 |
| [ ] | `supply_knitting_needles_4mm.png` | 4mm Knitting Needles | 512 x 512 |
| [ ] | `supply_knitting_needles_5mm.png` | 5mm Knitting Needles | 512 x 512 |
| [ ] | `supply_knitting_needles_bamboo_5mm.png` | 5mm Bamboo Knitting Needles | 512 x 512 |
| [ ] | `supply_marking_pen.png` | Fabric Marking Pen | 512 x 512 |
| [ ] | `supply_pins.png` | Box of Pins | 512 x 512 |
| [ ] | `supply_qsnap_frame.png` | Q-Snap Frame (holds 16 x 16 stitches) | 512 x 512 |
| [ ] | `supply_quilting_needle.png` | Quilting Needles | 512 x 512 |
| [ ] | `supply_rotary_cutter.png` | Rotary Cutter and Ruler | 512 x 512 |
| [ ] | `supply_safety_pins.png` | Quilter's Safety Pins | 512 x 512 |
| [ ] | `supply_scissors_sharp.png` | Sharp Dressmaker's Shears | 512 x 512 |
| [ ] | `supply_sewing_machine.png` | Great-Aunt's Sewing Machine | 512 x 512 |
| [ ] | `supply_sewing_machine_quilter.png` | Quilter's Sewing Machine | 512 x 512 |
| [ ] | `supply_sewing_needle.png` | Hand Sewing Needles | 512 x 512 |
| [ ] | `supply_sewing_needle_sharps.png` | Fine Sharps Needles | 512 x 512 |
| [ ] | `supply_stretcher_bars.png` | Stretcher Bars | 512 x 512 |
| [ ] | `supply_tapestry_needle.png` | Tapestry Needle | 512 x 512 |
| [ ] | `supply_tapestry_needle_bent.png` | Bent-Tip Tapestry Needle | 512 x 512 |

**Books (5)**

| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `supply_book_cables.png` | Cables and Arans | 512 x 512 |
| [ ] | `supply_book_knots.png` | A Little Book of Knots | 512 x 512 |
| [ ] | `supply_book_needlepoint.png` | Needlepoint for Cross Stitchers | 512 x 512 |
| [ ] | `supply_book_pattern_design.png` | The Pattern Maker's Notebook | 512 x 512 |
| [ ] | `supply_book_weaving.png` | Weaving on a Frame Loom | 512 x 512 |

## Part 8: Shop decoration and upgrades

Decor (folder `Art/Decor/`, replace placeholders; one picture is used in the shop and on the shop tile):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `decor_brass_lamp.png` | Brass Lamp (goes on: Counter) | 512 x 512 |
| [ ] | `decor_button_jar.png` | Jar of Buttons (goes on: Counter) | 512 x 512 |
| [ ] | `decor_counter_bell.png` | Brass Counter Bell (goes on: Counter, left end) | 512 x 512 |
| [ ] | `decor_cuckoo_clock.png` | Cuckoo Clock (goes on: Wall) | 512 x 512 |
| [ ] | `decor_dress_form.png` | Dress Form (goes on: Corner) | 512 x 512 |
| [ ] | `decor_fern.png` | Tall Fern (goes on: Corner) | 512 x 512 |
| [ ] | `decor_grandfather_clock.png` | Grandfather Clock (goes on: Corner) | 512 x 512 |
| [ ] | `decor_harbour_painting.png` | Harbour Painting (goes on: Wall) | 512 x 512 |
| [ ] | `decor_knitted_pouffe.png` | Knitted Pouffe (goes on: Floor) | 512 x 512 |
| [ ] | `decor_lighthouse_painting.png` | Lighthouse Painting (goes on: Pickup corner wall) | 512 x 512 |
| [ ] | `decor_parcel_basket.png` | Wicker Parcel Basket (goes on: Pickup corner shelf) | 512 x 512 |
| [ ] | `decor_patchwork_rug.png` | Patchwork Rug (goes on: Floor) | 512 x 512 |
| [ ] | `decor_pickup_bunting.png` | Pickup Bunting (goes on: Pickup corner wall) | 512 x 512 |
| [ ] | `decor_rag_rug.png` | Rag Rug (goes on: Floor) | 512 x 512 |
| [ ] | `decor_reading_lamp.png` | Green Reading Lamp (goes on: Counter) | 512 x 512 |
| [ ] | `decor_ribbon_reel.png` | Ribbon Reel Stand (goes on: Counter, left end) | 512 x 512 |
| [ ] | `decor_sampler_frame.png` | Framed Sampler (goes on: Wall) | 512 x 512 |
| [ ] | `decor_sewing_box.png` | Victorian Sewing Box (goes on: Counter, left end) | 512 x 512 |
| [ ] | `decor_shell_mobile.png` | Seashell Mobile (goes on: Pickup corner shelf) | 512 x 512 |
| [ ] | `decor_ships_bell.png` | Ship's Bell (goes on: Wall) | 512 x 512 |
| [ ] | `decor_string_spool.png` | Big Spool of Parcel String (goes on: Pickup corner shelf) | 512 x 512 |
| [ ] | `decor_teacup_plant.png` | Plant in a Teacup (goes on: Counter) | 512 x 512 |
| [ ] | `decor_teapot_set.png` | Teapot Collection (goes on: Floor) | 512 x 512 |
| [ ] | `decor_tide_chart.png` | Old Tide Chart (goes on: Wall) | 512 x 512 |
| [ ] | `decor_umbrella_stand.png` | Umbrella Stand (goes on: Corner) | 512 x 512 |

Upgrades (folder `Art/Upgrades/`, replace placeholders):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `upgrade_display_case.png` | Display Case | 512 x 512 |
| [ ] | `upgrade_display_shelf.png` | Display Shelf | 512 x 512 |
| [ ] | `upgrade_shop_sign.png` | Shop Sign | 512 x 512 |
| [ ] | `upgrade_work_lamp.png` | Work Lamp | 512 x 512 |

Shop furniture (folder `Art/Shop/`, new, transparent):
| | File name | What | Size (px) |
|---|---|---|---|
| [ ] | `shop_counter.png` | The counter | 2048 x 800 |
| [ ] | `shop_door_closed.png` | The door, closed | 1024 x 2048 |
| [ ] | `shop_door_open.png` | The door, open | 1024 x 2048 |
| [ ] | `shop_display_spot.png` | A pattern display spot (frame or stand) | 512 x 512 |
| [ ] | `shop_pickup_shelves.png` | Pickup corner shelving | 2048 x 1600 |
| [ ] | `shop_computer.png` | The computer on the counter (online orders) | 512 x 512 |
| [ ] | `shop_kettle.png` | Kettle (Put the Kettle On) | 256 x 256 |
| [ ] | `shop_dust_speck.png` | Dust speck (chores) | 128 x 128 |
| [ ] | `stall_table.png` | The Sunday stall table | 2048 x 800 |
| [ ] | `stall_awning.png` | The stall awning | 2048 x 400 |

## Totals (roughly)

| Part | Drawings |
|---|---|
| UI mock-ups | 38 |
| UI pieces, icons, pins, stamps, logo | about 90 |
| Animations | 7 short flipbooks |
| Backgrounds | 18 (+15 optional) |
| Characters | 150 (+ 4 fair regulars, 8 strangers) |
| Player character | 79 layers + 7 boutique tiles |
| Items | 24 |
| Crafting | about 65 |
| Supplies | 112 |
| Decor, upgrades, furniture | 39 |

Suggested order: the shared UI pieces (1b), `bg_shop_day.png`, and Mara, Tobias and Juno. Then the first day looks
finished, and the rest can follow.

// Poppy Wren (she/her): runs Wren's Pet Emporium on Harbour Lane (owner, 2026-11-02: pets for the shop, from a pet store).
// Adult; friendship only. Brisk, warm, always has a creature tucked somewhere about her. Opens at level 4.
// Chat with her in her shop (once a day); buying from her counts too. The player's name and pronouns: {name}, {they}...

=== poppy_intro ===
A woman in wellies is bottle-feeding a lamb behind the counter. A parrot on her shoulder says "Shop!" # happy
Poppy: Don't mind Admiral, he thinks he works here. Poppy Wren. # happy
~ met()
~ note(Runs Wren's Pet Emporium on Harbour Lane.)
Poppy: You're the one who opened the haberdashery? A shop wants a cat. Or a dog. Somebody to sit on the wool. # thoughtful
-> END

=== poppy_greet ===
Poppy: Mind the tortoise, he's going nowhere fast. Looking for a shop companion? # happy
-> END

=== poppy_greet_2_friend ===
Poppy: {name}! Admiral, say hello. # happy
Poppy: ...He won't, he's sulking. Come in, come in.
-> END

=== poppy_bye ===
Poppy: Off you go then. Give your lot a scratch behind the ears from me. # happy
-> END

=== poppy_thanks ===
Poppy: Look after each other. And bring them back to say hello! # happy
-> END

=== poppy_visit ===
Poppy: Every animal in here has a name and an opinion. Mostly the opinion is "feed me". # happy
-> END

=== poppy_visit_lamb ===
Poppy: The lamb's an orphan from Trelawney's farm. She thinks I'm her mum now. I've stopped arguing. # thoughtful
~ note(Is hand-rearing an orphan lamb from a farm up the hill.)
-> END

=== poppy_visit_admiral ===
Poppy: Admiral was a ship's parrot. He swears in four languages. I only understand two of them. # happy
~ note(Her parrot, Admiral, was a ship's parrot.)
-> END

=== poppy_scene_1 ===
Poppy is untangling a kitten from a ball of wool. # happy
Poppy: This is why I never took up knitting. Too much competition.
~ note(Never took up knitting: "too much competition" from the kittens.)
-> END

=== poppy_scene_2 ===
Poppy: Your great-aunt used to have a big grey cat called Bobbin. Slept on the counter like a loaf of bread. # thoughtful
Poppy: When he passed, she said the shop felt too quiet. I think that's when she started talking to the customers more.
~ note(Remembers Aunt Win's grey cat, Bobbin.)
-> END

=== poppy_scene_3 ===
Poppy: I'll tell you a secret. I can't keep the animals. I find them all homes, and then I miss every single one. # sad
Poppy: So when you come in and tell me how yours is getting on... it means a lot. Thank you, {name}. # happy
~ note(Misses every animal she finds a home for.)
-> END

=== poppy_saturday ===
You walk the dogs from the Emporium along the beach with Poppy. Admiral rides on her shoulder the whole way. # happy
Poppy: Best part of the week. Don't tell the cats.
-> END

=== poppy_gift ===
Poppy: For me? Oh, that's lovely. I'll have to hide it from the kittens. # happy
-> END

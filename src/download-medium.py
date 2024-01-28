from faster_whisper.utils import download_model


for size in ['medium']:
    model_path = download_model(
        size,
        local_files_only=False,
        cache_dir=None,
    )

# Auto-fix common CocoaPods build issues for Unity iOS projects.
# This file is appended to the Podfile before `pod install` by the Fastlane lane.
# It handles: deployment targets, bitcode, code signing, Swift versions, module maps.

post_install do |installer|
  min_ios = '13.0'

  installer.pods_project.targets.each do |target|
    target.build_configurations.each do |config|
      # Deployment target — raise to project minimum if pod requires lower
      current = config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] || '11.0'
      if Gem::Version.new(current) < Gem::Version.new(min_ios)
        config.build_settings['IPHONEOS_DEPLOYMENT_TARGET'] = min_ios
      end

      # Bitcode disabled — Xcode 14+ dropped support
      config.build_settings['ENABLE_BITCODE'] = 'NO'

      # Code signing — disable for pod targets (only the app target signs)
      config.build_settings['CODE_SIGNING_ALLOWED'] = 'NO'

      # Swift version — default to 5.0 if not set (prevents "module requires Swift X" errors)
      config.build_settings['SWIFT_VERSION'] ||= '5.0'

      # Module maps — required by Xcode 15/16 strict linker
      config.build_settings['CLANG_ENABLE_MODULES'] = 'YES'

      # Architecture — exclude i386 from simulator (no longer supported)
      config.build_settings['EXCLUDED_ARCHS[sdk=iphonesimulator*]'] = 'i386'
    end
  end
end
